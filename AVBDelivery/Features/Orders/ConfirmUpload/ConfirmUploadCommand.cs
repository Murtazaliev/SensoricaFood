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

namespace AVBDelivery.Features.Orders.ConfirmUpload
{
    public record ConfirmUploadCommand : IRequest<ConfirmUploadResult>;

    public class ConfirmUploadResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public class ConfirmUploadCommandHandler : IRequestHandler<ConfirmUploadCommand, ConfirmUploadResult>
    {
        private readonly ApplicationContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IDistributedCache _cache;
        private readonly AmoCrm _amoCrm;

        public ConfirmUploadCommandHandler(
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

        public async Task<ConfirmUploadResult> Handle(ConfirmUploadCommand request, CancellationToken ct)
        {
            var user = await _currentUser.GetUserAsync();

            var cachedJson = await _cache.GetStringAsync(string.Format(OrderConstants.CacheKeys.UploadPreview, user!.Id), ct);
            if (string.IsNullOrEmpty(cachedJson))
            {
                return new ConfirmUploadResult { Success = false, Message = OrderConstants.Messages.UploadPreviewExpired };
            }

            var model = JsonSerializer.Deserialize<OrderUploadPreviewViewModel>(cachedJson);
            await _cache.RemoveAsync(string.Format(OrderConstants.CacheKeys.UploadPreview, user.Id), ct);

            if (model == null || model.OrderGroups.Count == 0)
            {
                return new ConfirmUploadResult { Success = false, Message = OrderConstants.Messages.UploadNoData };
            }

            var createdOrderIds = new List<int>();

            foreach (var group in model.OrderGroups.Where(g => g.OrganizationFound))
            {
                var validItems = group.Items.Where(i => i.IsFound && i.Quantity > 0).ToList();
                if (validItems.Count == 0) continue;

                try
                {
                    var company = await _context.Organizations
                        .Include(o => o.Notes)
                        .FirstOrDefaultAsync(o => o.OrganizationId == group.OrganizationId, ct);
                    if (company == null) continue;

                    double discount = OrderHelper.GetDiscount(company);

                    var order = new Order
                    {
                        UserId = user.Id,
                        OrganizationId = group.OrganizationId,
                        OrderDate = DateTime.Now,
                        DeliveryDate = DateTime.Today.AddDays(1),
                        Items = validItems.Select(i => new OrderItem
                        {
                            ProductId = i.ProductId,
                            ProductName = i.ProductName,
                            Count = i.Quantity,
                            Price = Math.Round(i.Price ?? 0, 2),
                            MeasureUnit = i.MeasureUnit,
                            AmoCrmId = i.AmoCrmId
                        }).ToList(),
                        Sum = validItems.Sum(i => Math.Round((double)(i.Price ?? 0) * i.Quantity, 2))
                    };

                    if (order.Sum < (company.MinimalSum ?? 0)) continue;

                    if (company.MenuId != null)
                    {
                        var allowedProductIds = await _context.MenuProducts
                            .Where(mp => mp.MenuId == company.MenuId)
                            .Select(mp => mp.ProductId)
                            .ToListAsync(ct);
                        var invalidItems = order.Items
                            .Where(i => !allowedProductIds.Contains(i.ProductId))
                            .ToList();
                        if (invalidItems.Count > 0) continue;
                    }

                    order.SumWithDiscount = discount != 1
                        ? Math.Round(order.Sum.Value * discount)
                        : order.Sum;

                    var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.UserId == user.Id, ct);
                    if (contact?.AmoCrmId == null || company.AmoCrmId == null) continue;

                    var catalogs = await _amoCrm.GetCatalogs();
                    var catalogId = catalogs?.Embedded.Catalogs?.FirstOrDefault(c => c.Type == OrderConstants.AmoCrm.CatalogTypeProducts)?.Id;

                    var leadCustomFields = (await _amoCrm.GetLeadsCustomFields()).Embedded.CustomFields;
                    var deliveryDateField = leadCustomFields.FirstOrDefault(f => f.Name == OrderConstants.AmoCrm.LeadFields.DeliveryDate);
                    var deliveryTimeField = leadCustomFields.FirstOrDefault(f => f.Name == OrderConstants.AmoCrm.LeadFields.DeliveryTimeWeekday);
                    var deliveryWeekendTimeField = leadCustomFields.FirstOrDefault(f => f.Name == OrderConstants.AmoCrm.LeadFields.DeliveryTimeWeekend);
                    var notesField = leadCustomFields.FirstOrDefault(f => f.Name == OrderConstants.AmoCrm.LeadFields.Notes);

                    var lead = new Lead
                    {
                        Price = (int)(order.SumWithDiscount),
                        Embedded = new LeadEmbedded
                        {
                            CatalogElements = Array.Empty<CatalogElement>(),
                            Contacts = [new LeadContact { Id = int.Parse(contact.AmoCrmId) }],
                            Companies = [new LeadCompany { Id = int.Parse(company.AmoCrmId) }]
                        }
                    };

                    var pipelines = await _amoCrm.GetLeadPipelines();
                    var newClientPipeline = pipelines.Embedded.Pipelines.FirstOrDefault(p => p.Id == OrderConstants.AmoCrm.NewClientPipelineId);
                    var userHaveOrder = await _context.Orders.AnyAsync(o => o.UserId == user.Id && o.Id != 0, ct);
                    if (!userHaveOrder && newClientPipeline != null)
                    {
                        lead.PipelineId = newClientPipeline.Id;
                    }

                    var customFieldsToCreate = new List<CustomFieldValues>();
                    if (deliveryDateField != null)
                    {
                        var dateUnix = new DateTimeOffset(order.DeliveryDate ?? DateTime.UtcNow).ToUnixTimeSeconds();
                        customFieldsToCreate.Add(new CustomFieldValues
                        {
                            FieldId = deliveryDateField.Id,
                            Values = [new ElementValue { Value = dateUnix.ToString() }]
                        });
                    }
                    if (deliveryTimeField != null)
                        customFieldsToCreate.Add(new CustomFieldValues { FieldId = deliveryTimeField.Id, Values = [new ElementValue { Value = company.DeliveryTime }] });
                    if (deliveryWeekendTimeField != null)
                        customFieldsToCreate.Add(new CustomFieldValues { FieldId = deliveryWeekendTimeField.Id, Values = [new ElementValue { Value = company.DeliveryWeekendTime }] });
                    if (notesField != null && company.Notes.Count > 0)
                        customFieldsToCreate.Add(new CustomFieldValues { FieldId = notesField.Id, Values = company.Notes.Select(n => new ElementValue { EnumId = n.AmoCrmId }).ToArray() });

                    if (customFieldsToCreate.Any())
                        lead.CustomFieldsValues = customFieldsToCreate.ToArray();

                    var createdLeadBody = await _amoCrm.CreateLeads([lead]);
                    var createdLead = createdLeadBody?.Embedded?.Leads?.FirstOrDefault();

                    if (createdLead != null && catalogId != null)
                    {
                        var links = validItems.Select(i => new Link
                        {
                            ToEntityId = i.AmoCrmId ?? 0,
                            ToEntityType = OrderConstants.AmoCrm.EntityTypeCatalogElements,
                            Metadata = new LinkMetadata { Catalog_id = catalogId ?? 0, Quantity = (float)Math.Round(i.Quantity, 2) }
                        }).ToList();
                        await _amoCrm.CreateLeadLink(links, createdLead.Id);
                        await _amoCrm.UpdateLeads([new Lead { Id = createdLead.Id, Price = (int)order.SumWithDiscount }]);
                    }

                    order.AmoCrmId = createdLead?.Id;
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync(ct);
                    createdOrderIds.Add(order.Id);

                    await _context.DBLog.AddAsync(new DBLog
                    {
                        DateTime = order.OrderDate,
                        Message = string.Format(OrderConstants.Log.MessageOrderCreatedViaUpload, group.ColumnHeader),
                        User = user.UserName,
                        Level = OrderConstants.Log.LevelInfo
                    }, ct);
                    await _context.SaveChangesAsync(ct);
                }
                catch
                {
                    // log error, skip this location
                }
            }

            if (createdOrderIds.Count > 0)
            {
                return new ConfirmUploadResult
                {
                    Success = true,
                    Message = string.Format(OrderConstants.Messages.UploadOrdersCreated, createdOrderIds.Count, string.Join(", №", createdOrderIds))
                };
            }

            return new ConfirmUploadResult
            {
                Success = false,
                Message = OrderConstants.Messages.UploadOrdersFailed
            };
        }
    }
}
