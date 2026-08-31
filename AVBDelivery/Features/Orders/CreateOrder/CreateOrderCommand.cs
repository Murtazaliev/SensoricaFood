using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AVBDelivery.Helpers;
using AVBDelivery.Interfaces;
using AVBDelivery.Jobs;
using AVBDelivery.Models;
using AVBDelivery.Models.AmoCrm;
using AVBDelivery.Models.AmoCrm.Requests;
using AVBDelivery.ViewModels;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace AVBDelivery.Features.Orders.CreateOrder
{
    public record CreateOrderCommand(OrderCreateViewModel Model) : IRequest<CreateOrderResult>;

    public class CreateOrderResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? OrderId { get; set; }
    }

    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResult>
    {
        private readonly ApplicationContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IDistributedCache _cache;
        private readonly AmoCrm _amoCrm;

        public CreateOrderCommandHandler(
            ApplicationContext context,
            ICurrentUserService currentUser,
            IDistributedCache cache,
            AmoCrm amoCrm)
        {
            _context = context;
            _currentUser = currentUser;
            _cache = cache;
            _amoCrm = amoCrm;
        }

        public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken ct)
        {
            var user = await _currentUser.GetUserAsync();
            var orderCreate = request.Model;
            orderCreate.Order!.Items = new();
            orderCreate.Order.Sum = 0;

            var today = DateTime.UtcNow.Date;
            var redisKey = string.Format(OrderConstants.CacheKeys.Cart, user!.Id, today);
            var cachedJson = await _cache.GetStringAsync(redisKey, ct);

            if (string.IsNullOrEmpty(cachedJson))
            {
                return new CreateOrderResult { Success = false, Message = OrderConstants.Messages.CartEmpty };
            }

            var shoppingCart = JsonSerializer.Deserialize<List<ShoppingCart>>(cachedJson);
            if (shoppingCart == null || shoppingCart.Count == 0)
            {
                return new CreateOrderResult { Success = false, Message = OrderConstants.Messages.CartEmpty };
            }

            var orgId = orderCreate.Order.OrganizationId;
            var company = await _context.Organizations
                .Include(o => o.Notes)
                .FirstOrDefaultAsync(o => o.OrganizationId == orgId, ct);
            double discount = OrderHelper.GetDiscount(company);

            foreach (var item in shoppingCart.GroupBy(p => p.ProductId))
            {
                var orderItem = new OrderItem
                {
                    Count = item.Sum(x => x.Count),
                    Price = Math.Round(item.FirstOrDefault()!.Price, 2),
                    ProductName = item.FirstOrDefault()!.ProductName,
                    ProductId = item.FirstOrDefault()!.ProductId,
                    MeasureUnit = item.FirstOrDefault()!.MeasureUnit,
                    AmoCrmId = item.FirstOrDefault()!.AmoCrmId
                };
                if (orderItem.Count > 0)
                {
                    orderCreate.Order.Items.Add(orderItem);
                    orderCreate.Order.Sum += Math.Round(orderItem.Price * orderItem.Count);
                }
            }

            if (company!.MenuId != null)
            {
                var allowedProductIds = await _context.MenuProducts
                    .Where(mp => mp.MenuId == company.MenuId)
                    .Select(mp => mp.ProductId)
                    .ToListAsync(ct);
                var invalidItems = orderCreate.Order.Items
                    .Where(i => !allowedProductIds.Contains(i.ProductId))
                    .Select(i => i.ProductName)
                    .ToList();
                if (invalidItems.Count > 0)
                {
                    return new CreateOrderResult
                    {
                        Success = false,
                        Message = string.Format(OrderConstants.Messages.MenuChanged, string.Join(", ", invalidItems))
                    };
                }
            }

            if (orderCreate.Order.Sum < (company.MinimalSum ?? 0))
            {
                return new CreateOrderResult
                {
                    Success = false,
                    Message = string.Format(OrderConstants.Messages.OrderBelowMinimum, orderCreate.Order.Sum, company.MinimalSum)
                };
            }

            orderCreate.Order.SumWithDiscount = discount != 0
                ? Math.Round(orderCreate.Order.Sum.Value * discount)
                : orderCreate.Order.Sum;

            orderCreate.Order.UserId = user.Id;
            orderCreate.Order.OrderDate = DateTime.Now;

            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.UserId == user.Id, ct);
            if (contact?.AmoCrmId == null)
            {
                return new CreateOrderResult { Success = false, Message = OrderConstants.Messages.ContactMissing };
            }
            if (company.AmoCrmId == null)
            {
                return new CreateOrderResult { Success = false, Message = OrderConstants.Messages.CompanyMissing };
            }

            var catalogs = await _amoCrm.GetCatalogs();
            var catalogId = catalogs?.Embedded.Catalogs?.FirstOrDefault(c => c.Type == OrderConstants.AmoCrm.CatalogTypeProducts)?.Id;
            if (catalogId == null)
            {
                return new CreateOrderResult { Success = false, Message = OrderConstants.Messages.CatalogMissing };
            }

            var customFields = await _amoCrm.GetCustomFields(catalogId);
            var priceFieldId = customFields?.Embedded?.CustomFields?.FirstOrDefault(f => f.Code == OrderConstants.AmoCrm.FieldCodePrice)?.Id;
            if (priceFieldId == null)
            {
                return new CreateOrderResult { Success = false, Message = OrderConstants.Messages.PriceFieldMissing };
            }

            var leadCustomFields = (await _amoCrm.GetLeadsCustomFields()).Embedded.CustomFields;
            var deliveryDateField = leadCustomFields.FirstOrDefault(f => f.Name == OrderConstants.AmoCrm.LeadFields.DeliveryDate);
            var deliveryTimeField = leadCustomFields.FirstOrDefault(f => f.Name == OrderConstants.AmoCrm.LeadFields.DeliveryTimeWeekday);
            var deliveryWeekendTimeField = leadCustomFields.FirstOrDefault(f => f.Name == OrderConstants.AmoCrm.LeadFields.DeliveryTimeWeekend);
            var notesField = leadCustomFields.FirstOrDefault(f => f.Name == OrderConstants.AmoCrm.LeadFields.Notes);

            var lead = new Lead
            {
                Price = (int)(orderCreate.Order.SumWithDiscount),
                Embedded = new LeadEmbedded
                {
                    CatalogElements = Array.Empty<CatalogElement>(),
                    Contacts = [new LeadContact { Id = int.Parse(contact.AmoCrmId) }],
                    Companies = [new LeadCompany { Id = int.Parse(company.AmoCrmId) }]
                }
            };

            var pipelines = await _amoCrm.GetLeadPipelines();
            var newClientPipeline = pipelines.Embedded.Pipelines.FirstOrDefault(p => p.Id == OrderConstants.AmoCrm.NewClientPipelineId);
            var userHaveOrder = await _context.Orders.AnyAsync(o => o.UserId == user.Id, ct);

            if (!userHaveOrder && newClientPipeline != null)
            {
                lead.PipelineId = newClientPipeline.Id;
            }

            var customFieldsToCreate = new List<CustomFieldValues>();
            if (deliveryDateField != null)
            {
                var deliveryDate = orderCreate.Order.DeliveryDate ?? DateTime.Now;
                customFieldsToCreate.Add(new CustomFieldValues
                {
                    FieldId = deliveryDateField.Id,
                    Values = [new ElementValue { Value = new DateTimeOffset(deliveryDate).ToString("yyyy-MM-ddTHH:mm:sszzz") }]
                });
            }
            if (deliveryTimeField != null)
            {
                customFieldsToCreate.Add(new CustomFieldValues
                {
                    FieldId = deliveryTimeField.Id,
                    Values = [new ElementValue { Value = company.DeliveryTime }]
                });
            }
            if (deliveryWeekendTimeField != null)
            {
                customFieldsToCreate.Add(new CustomFieldValues
                {
                    FieldId = deliveryWeekendTimeField.Id,
                    Values = [new ElementValue { Value = company.DeliveryWeekendTime }]
                });
            }
            if (notesField != null && company.Notes.Count > 0)
            {
                customFieldsToCreate.Add(new CustomFieldValues
                {
                    FieldId = notesField.Id,
                    Values = company.Notes.Select(n => new ElementValue { EnumId = n.AmoCrmId }).ToArray()
                });
            }
            if (customFieldsToCreate.Any())
            {
                lead.CustomFieldsValues = customFieldsToCreate.ToArray();
            }

            var createdLeadBody = await _amoCrm.CreateLeads([lead]);
            if (createdLeadBody == null)
            {
                return new CreateOrderResult { Success = false, Message = OrderConstants.Messages.OrderCreateFailed };
            }
            var createdLead = createdLeadBody.Embedded.Leads.FirstOrDefault();

            if (createdLeadBody != null)
            {
                var catalog = (await _amoCrm.GetCatalogs()).Embedded.Catalogs?.FirstOrDefault(c => c.Type == OrderConstants.AmoCrm.CatalogTypeProducts)?.Id;
                if (catalog != null)
                {
                    var links = orderCreate.Order.Items.Select(item => new Link
                    {
                        ToEntityId = item.AmoCrmId ?? 0,
                        ToEntityType = OrderConstants.AmoCrm.EntityTypeCatalogElements,
                        Metadata = new LinkMetadata
                        {
                            Catalog_id = catalogId ?? 0,
                            Quantity = (float)Math.Round(item.Count, 2)
                        }
                    }).ToList();

                    var createdLink = await _amoCrm.CreateLeadLink(links, createdLead!.Id);
                    if (createdLink == null)
                    {
                        orderCreate.Order.AmoCrmId = createdLead.Id;
                        _context.Orders.Add(orderCreate.Order);
                        await _cache.RemoveAsync(redisKey, ct);
                        await _context.SaveChangesAsync(ct);
                        return new CreateOrderResult
                        {
                            Success = true,
                            Message = string.Format(OrderConstants.Messages.OrderCreatedButLinkFailed, orderCreate.Order.Id),
                            OrderId = orderCreate.Order.Id
                        };
                    }
                }

                var updatedLead = new Lead
                {
                    Id = createdLead!.Id,
                    Price = (int)orderCreate.Order.SumWithDiscount
                };
                await _amoCrm.UpdateLeads([updatedLead]);
            }

            await _cache.RemoveAsync(redisKey, ct);
            orderCreate.Order.AmoCrmId = createdLead!.Id;
            _context.Orders.Add(orderCreate.Order);
            await _context.SaveChangesAsync(ct);

            var dbLog = new DBLog
            {
                DateTime = orderCreate.Order.OrderDate,
                Message = OrderConstants.Log.MessageOrderCreated,
                User = user.UserName,
                Level = OrderConstants.Log.LevelInfo
            };
            await _context.DBLog.AddAsync(dbLog, ct);
            await _context.SaveChangesAsync(ct);

            return new CreateOrderResult
            {
                Success = true,
                Message = string.Format(OrderConstants.Messages.OrderCreated, orderCreate.Order.Id),
                OrderId = orderCreate.Order.Id
            };
        }
    }
}
