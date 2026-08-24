using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Printing;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using AVBDelivery.Helpers;
using AVBDelivery.Jobs;
using AVBDelivery.Models;
using AVBDelivery.Models.AmoCrm;
using AVBDelivery.Models.AmoCrm.Requests;
using AVBDelivery.ViewModels;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MimeKit.Encodings;
using NetVips;
using NuGet.Packaging;
using Org.BouncyCastle.Math.EC.Rfc7748;
using Polly;

namespace AVBDelivery.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<User> _userManager;
        private ILogger<OrdersController> _logger;
        private readonly IDistributedCache _cache;
        private AmoCrm _amoCrm;
        User _User;
        IList<string> _Roles;

        public OrdersController(ApplicationContext context, UserManager<User> userManager, AmoCrm amoCrm, IDistributedCache cache, ILogger<OrdersController> logger)
        {
            _context = context;
            _userManager = userManager;
            _amoCrm = amoCrm;
            _logger = logger;
            _cache = cache;
        }

        // GET: Orders
        [Authorize(Roles = "client")]
        public async Task<IActionResult> Index(string? startDate, string? endDate, string? organizationId, int page = 1)
        {
            await GetUserInfo();
            int pageSize = 20;
            var organizations = new List<Organization>();
            if (_Roles.Contains("admin"))
            {
                organizations = await _context.Organizations.ToListAsync();
            }
            else
            {
                organizations = await _context.Organizations
                    .Where(o => o.Contacts.FirstOrDefault(c => c.UserId == _User.Id) != null)
                    .ToListAsync();
            }
            var model = new OrdersViewModel
            {
                Organizations = organizations
            };
            Expression<Func<Order, bool>> predicate = p => p.UserId == _User.Id;

            if (startDate != null)
            {
                DateTime dtStartDate = Convert.ToDateTime(startDate);
                predicate = predicate.And(x => x.OrderDate >= dtStartDate);
            }
            if (endDate != null)
            {
                DateTime dtEndDate = Convert.ToDateTime(endDate);
                predicate = predicate.And(x => x.OrderDate <= dtEndDate.AddDays(1));
            }
            if (organizationId != null)
            {
                predicate = predicate.And(x => x.OrganizationId == organizationId);
            }
            

            var totalOrders = await _context.Orders
                .Where(predicate)
                .CountAsync();

            var orders = await _context.Orders
                .Where(predicate)
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            var pageInfo = new PageInfo
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = totalOrders
            };
            model.Orders = orders;
            model.PageInfo = pageInfo;
            return View(model);
        }

        // GET: Orders
        [Authorize(Roles = "admin, operator")]
        public async Task<IActionResult> Report()
        {
            return View(await GetOrders());
        }

        async Task<List<Order>> GetOrders()
        {
            await GetUserInfo();

            var orders = await _context.Orders
                .Include(x => x.Items)
                .Where(x => x.OrderDate >= DateTime.Today & x.OrderDate < DateTime.Today.AddDays(1))
                .AsNoTracking()
                .ToListAsync();

            var userIds = orders.Select(o => o.UserId).Distinct().ToList();
            var userNames = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            foreach (var item in orders)
            {
                if (userNames.TryGetValue(item.UserId, out var userName))
                {
                    item.UserId = userName;
                }
            }
            return orders;
        }

        // GET: Orders/Details/5
        [Authorize(Roles = "client")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }
            await GetUserInfo();
            var order = await _context.Orders.Include(x => x.Items).FirstOrDefaultAsync(m => m.Id == id & m.UserId == _User.Id);
            if (order == null)
            {
                return NotFound();
            }
            var address = (await _context.Organizations.FirstOrDefaultAsync(x => x.OrganizationId == order.OrganizationId))?.DeliveryAddress;
            var viewModel = new OrderViewModel
            {
                Order = order,
                Address = address
            };
            if (order.AmoCrmId != null)
            {
                var createdLead = await _amoCrm.GetLead(order.AmoCrmId ?? 0);
                var base1c = createdLead?.CustomFieldsValues?.FirstOrDefault(f => f.FieldId == 964597);
                var link = base1c?.Values.FirstOrDefault()?.Value;
                viewModel.Link = link?.ToString();
            }
            
            
            
            return View(viewModel);
        }

        [Authorize(Roles = "client")]
        public async Task<IActionResult> Create()
        {
            await GetUserInfo();
            var order = new Order
            {
                Items = new List<OrderItem>(),
                Sum = 0
            };

            var today = DateTime.UtcNow.Date;
            var cacheKey = $"cart:{_User.Id}:{today:yyyy-MM-dd}";
            var cachedJson = await _cache.GetStringAsync(cacheKey);

            List<string> notAddedItems = new();

            if (TempData.TryGetValue("NotAdded", out object? value))
            {
                try
                {
                    notAddedItems = JsonSerializer.Deserialize<List<string>>(value as string);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
            }

            if (!string.IsNullOrEmpty(cachedJson))
            {
                var shoppingCart = JsonSerializer.Deserialize<List<ShoppingCart>>(cachedJson);

                foreach (var itemGroup in shoppingCart.GroupBy(p => p.ProductId))
                {
                    var first = itemGroup.First();

                    var orderItem = new OrderItem
                    {
                        Id = first.Id,
                        Count = itemGroup.Sum(x => x.Count),
                        Price = Math.Round(first.Price, 2),
                        ProductName = first.ProductName,
                        ProductId = first.ProductId,
                        MeasureUnit = first.MeasureUnit,
                        AmoCrmId = first.AmoCrmId
                    };

                    if (orderItem.Count > 0)
                    {
                        order.Items.Add(orderItem);
                        order.Sum += Math.Round(orderItem.Price * orderItem.Count, 2);
                    }
                }
            }

            var organizations = await _context.Organizations
                .Where(o => o.Contacts.Any(c => c.UserId == _User.Id))
                .ToListAsync();

            var todayDate = DateOnly.FromDateTime(DateTime.Today);
            var lastDeliveryDate = todayDate.AddDays(7);
            List<DateOnly> dates = new();

            for (var date = todayDate.AddDays(1); date <= lastDeliveryDate; date = date.AddDays(1))
                dates.Add(date);

            var ret = new OrderCreateViewModel
            {
                Dates = dates,
                Order = order,
                Organizations = organizations,
                NotAddedItems = notAddedItems
            };

            return View(ret);
        }

        //// GET: Orders/Create
        //[Authorize(Roles = "client")]
        //public async Task<IActionResult> Create()
        //{
        //    await GetUserInfo();
        //    var order = new Order();
        //    order.Items = new();
        //    order.Sum = 0;
        //    var shoppingCart = await _context.ShoppingCart.Where(x => x.UserId == _User.Id & x.OrderDate >= DateTime.Today & x.OrderDate < DateTime.Today.AddDays(1)).ToListAsync();

        //    // Объединяем общие позиции
        //    foreach (var item in shoppingCart.GroupBy(p => p.ProductId))
        //    {
        //        OrderItem orderItem = new()
        //        {
        //            Id = item.FirstOrDefault().Id,
        //            Count = item.Sum(x => x.Count),
        //            Price = Math.Round(item.FirstOrDefault().Price, 2),
        //            ProductName = item.FirstOrDefault().ProductName,
        //            ProductId = item.FirstOrDefault().ProductId,
        //            MeasureUnit = item.FirstOrDefault().MeasureUnit,
        //            AmoCrmId = item.FirstOrDefault().AmoCrmId
        //        };
        //        if (orderItem.Count > 0)
        //        {
        //            order.Items.Add(orderItem);
        //            order.Sum += Math.Round(orderItem.Price * orderItem.Count, 2);
        //        }
        //    }
        //    //order.OrderDate = shoppingCart.FirstOrDefault().OrderDate;
        //    //order.UserId = _User.Id;
        //    var organizations = await _context.Organizations.Where(o => o.Contacts.FirstOrDefault(c => c.UserId == _User.Id) != null).ToListAsync();
        //    //var organizations = await _context.Organizations.Where(o => o.UserId == _User.Id).ToListAsync();

        //    var todayDate = DateOnly.FromDateTime(DateTime.Today);
        //    var lastDeliveryDate = todayDate.AddDays(7);
        //    List<DateOnly> dates = new List<DateOnly>();

        //    for (var date = todayDate.AddDays(1); date <= lastDeliveryDate; date = date.AddDays(1))
        //        dates.Add(date);


        //    //var dateOfComing = await _context.DateOfComing.ToListAsync();


        //    OrderCreateViewModel ret = new()
        //    {
        //        Dates = dates,
        //        Order = order,
        //        Organizations = organizations
        //    };

        //    return View(ret);
        //}


        [HttpPost]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> Create(OrderCreateViewModel orderCreate)
        {
            string errorMessages = "";
            foreach (var item in ModelState)
            {
                if (item.Value.ValidationState == ModelValidationState.Invalid)
                {
                    errorMessages += $"{errorMessages}\nОшибки для свойства {item.Key}:\n";
                    // пробегаемся по всем ошибкам
                    foreach (var error in item.Value.Errors)
                    {
                        errorMessages += $"{errorMessages}{error.ErrorMessage}\n";
                    }
                }
            }

            await GetUserInfo();
            if (ModelState.IsValid)
            {
                try
                {
                    orderCreate.Order.Items = new();
                    orderCreate.Order.Sum = 0;


                    var today = DateTime.UtcNow.Date;
                    var redisKey = $"cart:{_User.Id}:{today:yyyy-MM-dd}";
                    var cachedJson = await _cache.GetStringAsync(redisKey);

                    if (string.IsNullOrEmpty(cachedJson))
                    {
                        TempData["OrderResult.Success"] = false;
                        TempData["OrderResult.Message"] = "Корзина пуста — заказ не создан.";
                        return RedirectToAction(nameof(Index)); // Корзина пуста
                    }

                    var shoppingCart = JsonSerializer.Deserialize<List<ShoppingCart>>(cachedJson);
                    if (shoppingCart == null || shoppingCart.Count == 0)
                    {
                        TempData["OrderResult.Success"] = false;
                        TempData["OrderResult.Message"] = "Корзина пуста — заказ не создан.";
                        return RedirectToAction(nameof(Index)); // Корзина пуста
                    }


                    //var shoppingCart = await _context.ShoppingCart.Where(x => x.UserId == _User.Id & x.OrderDate >= DateTime.Today & x.OrderDate < DateTime.Today.AddDays(1)).ToListAsync();

                    //if (shoppingCart.Count == 0)
                    //{
                    //    return RedirectToAction(nameof(Index));
                    //}
                    var orgId = orderCreate.Order.OrganizationId;
                    var company = await _context.Organizations.Include(o => o.Notes).FirstOrDefaultAsync(o => o.OrganizationId == orgId);
                    double discount = OrderHelper.GetDiscount(company);

                    // Объединяем общие позиции
                    foreach (var item in shoppingCart.GroupBy(p => p.ProductId))
                    {
                        OrderItem orderItem = new()
                        {
                            Count = item.Sum(x => x.Count),
                            Price = Math.Round(item.FirstOrDefault().Price, 2),
                            ProductName = item.FirstOrDefault().ProductName,
                            ProductId = item.FirstOrDefault().ProductId,
                            MeasureUnit = item.FirstOrDefault().MeasureUnit,
                            AmoCrmId = item.FirstOrDefault().AmoCrmId
                        };
                        if (orderItem.Count > 0)
                        {
                            orderCreate.Order.Items.Add(orderItem);
                            orderCreate.Order.Sum += Math.Round(orderItem.Price * orderItem.Count);
                        }
                    }

                    if (company.MenuId != null)
                    {
                        var allowedProductIds = await _context.MenuProducts
                            .Where(mp => mp.MenuId == company.MenuId)
                            .Select(mp => mp.ProductId)
                            .ToListAsync();
                        var invalidItems = orderCreate.Order.Items
                            .Where(i => !allowedProductIds.Contains(i.ProductId))
                            .Select(i => i.ProductName)
                            .ToList();
                        if (invalidItems.Count > 0)
                        {
                            ModelState.AddModelError("Заказ не создан", $"Меню изменилось. Товары больше не доступны: {string.Join(", ", invalidItems)}. Обновите корзину.");
                            await WriteToLog(orderCreate, "В корзине товары, отсутствующие в меню организации");
                            return View(orderCreate);
                        }
                    }

                    if (orderCreate.Order.Sum < (company.MinimalSum ?? 0))
                    {
                        ModelState.AddModelError("Заказ не создан", $"Заказ не создан. Заказ меньше минимальной суммы. Сумма заказа: {orderCreate.Order.Sum}. Минимальная сумма: {company.MinimalSum}");
                        await WriteToLog(orderCreate, $"Заказ не создан. Заказ меньше минимальной суммы. Сумма заказа: {orderCreate.Order.Sum}. Минимальная сумма: {company.MinimalSum}");
                        return View(orderCreate);
                    }

                    if (discount != 0)
                    {
                        orderCreate.Order.SumWithDiscount = Math.Round(orderCreate.Order.Sum.Value * discount);
                    }
                    else
                    {
                        orderCreate.Order.SumWithDiscount = orderCreate.Order.Sum;
                    }

                    orderCreate.Order.UserId = _User.Id;
                    orderCreate.Order.OrderDate = DateTime.Now;


                    var catalogs = await _amoCrm.GetCatalogs();
                    var catalogId = catalogs?.Embedded.Catalogs?.FirstOrDefault(c => c.Type == "products")?.Id;
                    if (catalogId == null)
                    {
                        ModelState.AddModelError("Заказ не создан", "Заказ не создан. Отсутствует каталог товаров. Обратитесь к системному администратору");
                        await WriteToLog(orderCreate, "Отсутствует каталог товаров");
                        return View(orderCreate);
                    }
                    var customFields = await _amoCrm.GetCustomFields(catalogId);
                    var priceFieldId = customFields?.Embedded?.CustomFields?.FirstOrDefault(f => f.Code == "PRICE")?.Id;
                    if (priceFieldId == null)
                    {
                        ModelState.AddModelError("Заказ не создан", "Заказ не создан. Отсутствует поле цены. Обратитесь к системному администратору");
                        await WriteToLog(orderCreate, "Отсутствует поле цены");
                        return View(orderCreate);
                    }

                    List<CatalogElement> catalogElements = new List<CatalogElement>();

                    //var fullPrice = (int)Math.Round(orderCreate.Order.Items.Sum(i => i.Price));

                    //foreach (var item in orderCreate.Order.Items)
                    //{
                    //    var catalogElement = new CatalogElement
                    //    {
                    //        Id = item.AmoCrmId ?? 0,
                    //        Metadata = new Metadata
                    //        {
                    //            CatalogId = catalogId.ToString(),
                    //            Quantity = (int)item.Count,
                    //            PriceId = priceFieldId
                    //        }
                    //    };
                    //    catalogElements.Add(catalogElement);
                    //}

                    var userId = orderCreate.Order.UserId;
                    var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.UserId == userId);



                    var leadCustomFields = (await _amoCrm.GetLeadsCustomFields()).Embedded.CustomFields;
                    var deliveryDateField = leadCustomFields.FirstOrDefault(f => f.Name == "Дата доставки");
                    var deliveryCommentField = leadCustomFields.FirstOrDefault(f => f.Name == "Комментарий к заказу");
                    var deliveryTimeField = leadCustomFields.FirstOrDefault(f => f.Name == "Время для буднего");
                    var deliveryWeekendTimeField = leadCustomFields.FirstOrDefault(f => f.Name == "Время для выходного");
                    var notesField = leadCustomFields.FirstOrDefault(f => f.Name == "Примечание");


                    var deliveryDate = orderCreate.Order.DeliveryDate;
                    var deliveryTime = company?.DeliveryTime;
                    //var delivery = new DateTimeOffset(deliveryDate.Value.Add(deliveryTime.ToTimeSpan())).ToUnixTimeSeconds();
                    if (contact?.AmoCrmId == null)
                    {
                        ModelState.AddModelError("Заказ не создан", "Заказ не создан. Отсутствует контакт. Обратитесь к системному администратору");
                        await WriteToLog(orderCreate, "Отсутствует контакт");
                        return View(orderCreate);
                    }
                    if (company?.AmoCrmId == null)
                    {
                        ModelState.AddModelError("Заказ не создан", "Заказ не создан. Отсутствует компания. Обратитесь к системному администратору");
                        await WriteToLog(orderCreate, "Отсутствует компания");
                        return View(orderCreate);
                    }

                    var pipelines = await _amoCrm.GetLeadPipelines();
                    var newClientPipeline = pipelines.Embedded.Pipelines.FirstOrDefault(p => p.Id == 9457550); // Новые клиенты воронка
                    var userHaveOrder = await _context.Orders.AnyAsync(o => o.UserId == _User.Id);

                    

                    var lead = new Lead
                    {
                        Price = (int)(orderCreate.Order.SumWithDiscount),
                        Embedded = new LeadEmbedded
                        {
                            CatalogElements = catalogElements.ToArray(),
                            Contacts =
                            [
                                new LeadContact
                                {
                                    Id = int.Parse(contact.AmoCrmId)
                                }
                            ],
                            Companies =
                            [
                                new LeadCompany
                                {
                                    Id = int.Parse(company.AmoCrmId)
                                }
                            ]
                        }

                    };

                    if (!userHaveOrder && newClientPipeline != null)
                    {
                        lead.PipelineId = newClientPipeline.Id;
                    }

                    var customFieldsToCreate = new List<CustomFieldValues>();
                    if (deliveryDateField != null)
                    {
                        var dateUnix = new DateTimeOffset(orderCreate.Order.DeliveryDate ?? DateTime.UtcNow).ToUnixTimeSeconds();
                        customFieldsToCreate.Add(new CustomFieldValues
                        {
                            FieldId = deliveryDateField.Id,
                            Values = [
                                   new ElementValue
                                   {
                                       Value = orderCreate.Order.DeliveryDate.HasValue ? dateUnix : ""
                                   }
                               ]
                        });
                    }
                    if (deliveryTimeField != null)
                    {
                        customFieldsToCreate.Add(new CustomFieldValues
                        {
                            FieldId = deliveryTimeField.Id,
                            Values = [
                                   new ElementValue
                                   {
                                       Value = company.DeliveryTime
                                   }
                               ]
                        });
                    }
                    
                    if (deliveryWeekendTimeField != null)
                    {
                        customFieldsToCreate.Add(new CustomFieldValues
                        {
                            FieldId = deliveryWeekendTimeField.Id,
                            Values = [
                                   new ElementValue
                                   {
                                       Value = company.DeliveryWeekendTime
                                   }
                               ]
                        });
                    }

                    if (notesField != null && company.Notes.Count > 0)
                    {
                        customFieldsToCreate.Add(new CustomFieldValues
                        {
                            FieldId = notesField.Id,
                            Values = company.Notes.Select(n => new ElementValue
                            {
                                EnumId = n.AmoCrmId
                            })
                            .ToArray()
                        });
                    }

                    if (customFieldsToCreate.Any())
                    {
                        lead.CustomFieldsValues = customFieldsToCreate.ToArray();
                    }

                    var createdLeadBody = (await _amoCrm.CreateLeads([lead]));
                    if (createdLeadBody == null)
                    {
                        ModelState.AddModelError("Заказ не создан", "Заказ не создан. Обратитесь к системному администратору");
                        await WriteToLog(orderCreate, "Заказ не создан");
                        return View(orderCreate);
                    }
                    var createdLead = createdLeadBody.Embedded.Leads.FirstOrDefault();
                    var createdLeadLinks = (await _amoCrm.GetLeadLinks(createdLead.Id)).Embedded.Links.FirstOrDefault(l => l.ToEntityType == "catalog_elements");
                    if (createdLeadBody != null)
                    {
                        //var createdLead = createdLeadBody.
                        var catalog = (await _amoCrm.GetCatalogs()).Embedded.Catalogs?.FirstOrDefault(c => c.Type == "products")?.Id;
                        if (catalog != null)
                        {
                            var links = new List<Link>();
                            foreach (var item in orderCreate.Order.Items)
                            {
                                var link = new Link
                                {
                                    ToEntityId = item.AmoCrmId ?? 0,
                                    ToEntityType = "catalog_elements",
                                    Metadata = new LinkMetadata
                                    {
                                        Catalog_id = catalogId ?? 0,
                                        Quantity = (float)Math.Round(item.Count, 2)
                                    }
                                };
                                links.Add(link);
                            }
                            var createdLink = await _amoCrm.CreateLeadLink(links, createdLead.Id);
                            if (createdLink == null)
                            {
                                //ModelState.AddModelError("Заказ создан", "Заказ создан без товаров. Обратитесь к системному администратору");
                                orderCreate.Order.AmoCrmId = createdLead.Id;
                                _context.Orders.Add(orderCreate.Order);
                                await _cache.RemoveAsync(redisKey);
                                //_context.ShoppingCart.RemoveRange(shoppingCart);
                                await _context.SaveChangesAsync();
                                await WriteToLog(orderCreate, "Заказ создан без товаров");
                                TempData["OrderResult.Success"] = true;
                                TempData["OrderResult.Message"] = $"Заказ №{orderCreate.Order.Id} создан, но товары не прикрепились в amoCRM. Обратитесь к администратору";
                                TempData["OrderResult.OrderId"] = orderCreate.Order.Id;
                                return RedirectToAction(nameof(Index));
                            }
                        }
                        var updatedLead = new Lead
                        {
                            Id = createdLead.Id,
                            Price = (int)orderCreate.Order.SumWithDiscount
                        };
                        await _amoCrm.UpdateLeads([updatedLead]);
                    }

                    await _cache.RemoveAsync(redisKey);
                    orderCreate.Order.AmoCrmId = createdLead.Id;
                    _context.Orders.Add(orderCreate.Order);
                    //_context.ShoppingCart.RemoveRange(shoppingCart);
                    await _context.SaveChangesAsync();
                    //var additionalInfo = new List<string>();
                    var dbLog = new DBLog
                    {
                        DateTime = orderCreate.Order.OrderDate,
                        Message = "Создан заказ",
                        User = User.Identity.Name,
                        Level = "INFO"
                    };
                    await _context.DBLog.AddAsync(dbLog);
                    await _context.SaveChangesAsync();
                    TempData["OrderResult.Success"] = true;
                    TempData["OrderResult.Message"] = $"Заказ №{orderCreate.Order.Id} успешно создан.";
                    TempData["OrderResult.OrderId"] = orderCreate.Order.Id;

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await WriteToLog(orderCreate, ex.ToString());
                    ModelState.AddModelError("", "Не удалось создать заказ. Попробуйте ещё раз.");
                    return View(orderCreate);
                }
            }
            //orderCreate.Dates = await _context.DateOfComing.ToListAsync();

            return View(orderCreate);
        }


        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[Authorize(Roles = "client")]
        //public async Task<IActionResult> Create(OrderCreateViewModel orderCreate)
        //{
        //    string errorMessages = "";
        //    foreach (var item in ModelState)
        //    {
        //        if (item.Value.ValidationState == ModelValidationState.Invalid) 
        //        {
        //            errorMessages += $"{errorMessages}\nОшибки для свойства {item.Key}:\n";
        //            // пробегаемся по всем ошибкам
        //            foreach (var error in item.Value.Errors)
        //            {
        //                errorMessages += $"{errorMessages}{error.ErrorMessage}\n";
        //            }
        //        }
        //    }

        //    await GetUserInfo();
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            orderCreate.Order.Items = new();
        //            orderCreate.Order.Sum = 0;

        //            var shoppingCart = await _context.ShoppingCart.Where(x => x.UserId == _User.Id & x.OrderDate >= DateTime.Today & x.OrderDate < DateTime.Today.AddDays(1)).ToListAsync();

        //            if (shoppingCart.Count == 0)
        //            {
        //                return RedirectToAction(nameof(Index));
        //            }
        //            var orgId = orderCreate.Order.OrganizationId;
        //            var company = await _context.Organizations.FirstOrDefaultAsync(o => o.OrganizationId == orgId);
        //            double discount = OrderHelper.GetDiscount(company);

        //            // Объединяем общие позиции
        //            foreach (var item in shoppingCart.GroupBy(p => p.ProductId))
        //            {
        //                OrderItem orderItem = new()
        //                {
        //                    Count = item.Sum(x => x.Count),
        //                    Price = Math.Round(item.FirstOrDefault().Price, 2),
        //                    ProductName = item.FirstOrDefault().ProductName,
        //                    ProductId = item.FirstOrDefault().ProductId,
        //                    MeasureUnit = item.FirstOrDefault().MeasureUnit,
        //                    AmoCrmId = item.FirstOrDefault().AmoCrmId
        //                };
        //                if (orderItem.Count > 0)
        //                {
        //                    orderCreate.Order.Items.Add(orderItem);
        //                    orderCreate.Order.Sum += Math.Round(orderItem.Price * orderItem.Count);
        //                }
        //            }

        //            if (discount != 0)
        //            {
        //                orderCreate.Order.SumWithDiscount = Math.Round(orderCreate.Order.Sum.Value * discount);
        //            }
        //            else
        //            {
        //                orderCreate.Order.SumWithDiscount = orderCreate.Order.Sum;
        //            }

        //            orderCreate.Order.UserId = _User.Id;    
        //            orderCreate.Order.OrderDate = DateTime.Now; 


        //            var catalogs = await _amoCrm.GetCatalogs();
        //            var catalogId = catalogs?.Embedded.Catalogs?.FirstOrDefault(c => c.Type == "products")?.Id;
        //            if (catalogId == null)
        //            {
        //                ModelState.AddModelError("Заказ не создан", "Заказ не создан. Отсутствует каталог товаров. Обратитесь к системному администратору");
        //                await WriteToLog(orderCreate, "Отсутствует каталог товаров");
        //                return View(orderCreate);
        //            }
        //            var customFields = await _amoCrm.GetCustomFields(catalogId);
        //            var priceFieldId = customFields?.Embedded?.CustomFields?.FirstOrDefault(f => f.Code == "PRICE")?.Id;
        //            if (priceFieldId == null)
        //            {
        //                ModelState.AddModelError("Заказ не создан", "Заказ не создан. Отсутствует поле цены. Обратитесь к системному администратору");
        //                await WriteToLog(orderCreate, "Отсутствует поле цены");
        //                return View(orderCreate);
        //            }

        //            List<CatalogElement> catalogElements = new List<CatalogElement>();

        //            //var fullPrice = (int)Math.Round(orderCreate.Order.Items.Sum(i => i.Price));

        //            //foreach (var item in orderCreate.Order.Items)
        //            //{
        //            //    var catalogElement = new CatalogElement
        //            //    {
        //            //        Id = item.AmoCrmId ?? 0,
        //            //        Metadata = new Metadata
        //            //        {
        //            //            CatalogId = catalogId.ToString(),
        //            //            Quantity = (int)item.Count,
        //            //            PriceId = priceFieldId
        //            //        }
        //            //    };
        //            //    catalogElements.Add(catalogElement);
        //            //}

        //            var userId = orderCreate.Order.UserId;
        //            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.UserId == userId);



        //            var leadCustomFields = (await _amoCrm.GetLeadsCustomFields()).Embedded.CustomFields;
        //            var deliveryDateField = leadCustomFields.FirstOrDefault(f => f.Name == "Даты доставки");
        //            var deliveryCommentField = leadCustomFields.FirstOrDefault(f => f.Name == "Комментарий к заказу");
        //            var deliveryTimeField = leadCustomFields.FirstOrDefault(f => f.Name == "Время для буднего");
        //            var deliveryWeekendTimeField = leadCustomFields.FirstOrDefault(f => f.Name == "Время для выходного");

        //            var deliveryDate = orderCreate.Order.DeliveryDate;
        //            var deliveryTime = company?.DeliveryTime;
        //            //var delivery = new DateTimeOffset(deliveryDate.Value.Add(deliveryTime.ToTimeSpan())).ToUnixTimeSeconds();
        //            if (contact?.AmoCrmId == null)
        //            {
        //                ModelState.AddModelError("Заказ не создан", "Заказ не создан. Отсутствует контакт. Обратитесь к системному администратору");
        //                await WriteToLog(orderCreate, "Отсутствует контакт");
        //                return View(orderCreate);
        //            }
        //            if (company?.AmoCrmId == null)
        //            {
        //                ModelState.AddModelError("Заказ не создан", "Заказ не создан. Отсутствует компания. Обратитесь к системному администратору");
        //                await WriteToLog(orderCreate, "Отсутствует компания");
        //                return View(orderCreate);
        //            }
        //            var lead = new Lead
        //            {
        //                Price = (int)(orderCreate.Order.SumWithDiscount),
        //                Embedded = new LeadEmbedded
        //                {
        //                    CatalogElements = catalogElements.ToArray(),
        //                    Contacts =
        //                    [
        //                        new LeadContact
        //                        {
        //                            Id = int.Parse(contact.AmoCrmId)
        //                        }
        //                    ],
        //                    Companies =
        //                    [
        //                        new LeadCompany
        //                        {
        //                            Id = int.Parse(company.AmoCrmId)
        //                        }
        //                    ]
        //                }

        //            };

        //            var customFieldsToCreate = new List<CustomFieldValues>();
        //            if (deliveryDateField != null)
        //            {
        //                customFieldsToCreate.Add(new CustomFieldValues
        //                {
        //                    FieldId = deliveryDateField.Id,
        //                    Values = [
        //                           new ElementValue
        //                           {
        //                               Value = orderCreate.Order.DeliveryDate.HasValue ? orderCreate.Order.DeliveryDate.Value.ToString("dd.MM.yyyy") : ""
        //                           }
        //                       ]
        //                });
        //            }
        //            if (deliveryTimeField != null)
        //            {
        //                customFieldsToCreate.Add(new CustomFieldValues
        //                {
        //                    FieldId = deliveryTimeField.Id,
        //                    Values = [
        //                           new ElementValue
        //                           {
        //                               Value = company.DeliveryTime
        //                           }
        //                       ]
        //                });
        //            };
        //            if (deliveryWeekendTimeField != null)
        //            {
        //                customFieldsToCreate.Add(new CustomFieldValues
        //                {
        //                    FieldId = deliveryWeekendTimeField.Id,
        //                    Values = [
        //                           new ElementValue
        //                           {
        //                               Value = company.DeliveryWeekendTime
        //                           }
        //                       ]
        //                });
        //            }

        //            if (customFieldsToCreate.Any())
        //            {
        //                lead.CustomFieldsValues = customFieldsToCreate.ToArray();
        //            }

        //            var createdLeadBody = (await _amoCrm.CreateLeads([lead]));
        //            if (createdLeadBody == null)
        //            {
        //                ModelState.AddModelError("Заказ не создан", "Заказ не создан. Обратитесь к системному администратору");
        //                await WriteToLog(orderCreate, "Заказ не создан");
        //                return View(orderCreate);
        //            }
        //            var createdLead = createdLeadBody.Embedded.Leads.FirstOrDefault();
        //            var createdLeadLinks = (await _amoCrm.GetLeadLinks(createdLead.Id)).Embedded.Links.FirstOrDefault(l => l.ToEntityType == "catalog_elements");
        //            if (createdLeadBody != null)
        //            {
        //                //var createdLead = createdLeadBody.
        //                var catalog = (await _amoCrm.GetCatalogs()).Embedded.Catalogs?.FirstOrDefault(c => c.Type == "products")?.Id;
        //                if (catalog != null)
        //                {
        //                    var links = new List<Link>();
        //                    foreach (var item in orderCreate.Order.Items)
        //                    {
        //                        var link = new Link
        //                        {
        //                            ToEntityId = item.AmoCrmId ?? 0,
        //                            ToEntityType = "catalog_elements",
        //                            Metadata = new LinkMetadata
        //                            {
        //                                Catalog_id = catalogId ?? 0,
        //                                Quantity = (float)Math.Round(item.Count, 2)
        //                            }
        //                        };
        //                        links.Add(link);
        //                    }
        //                    var createdLink = await _amoCrm.CreateLeadLink(links, createdLead.Id);
        //                    if (createdLink == null)
        //                    {
        //                        //ModelState.AddModelError("Заказ создан", "Заказ создан без товаров. Обратитесь к системному администратору");
        //                        _context.Orders.Add(orderCreate.Order);
        //                        _context.ShoppingCart.RemoveRange(shoppingCart);
        //                        await _context.SaveChangesAsync();
        //                        await WriteToLog(orderCreate, "Заказ создан без товаров");
        //                        return RedirectToAction(nameof(Index));
        //                    }
        //                }
        //                var updatedLead = new Lead
        //                {
        //                    Id = createdLead.Id,
        //                    Price = (int)orderCreate.Order.SumWithDiscount
        //                };
        //                await _amoCrm.UpdateLeads([updatedLead]);
        //            }
        //            _context.Orders.Add(orderCreate.Order);
        //            _context.ShoppingCart.RemoveRange(shoppingCart);
        //            await _context.SaveChangesAsync();
        //            //var additionalInfo = new List<string>();
        //            var dbLog = new DBLog
        //            {
        //                DateTime = orderCreate.Order.OrderDate,
        //                Message = "Создан заказ",
        //                User = User.Identity.Name,
        //                Level = "INFO"
        //            };
        //            await _context.DBLog.AddAsync(dbLog);
        //            await _context.SaveChangesAsync();
        //            return RedirectToAction(nameof(Index));
        //        }
        //        catch (Exception ex)
        //        {
        //            await WriteToLog(orderCreate, ex.ToString());
        //        }
        //    }
        //    //orderCreate.Dates = await _context.DateOfComing.ToListAsync();

        //    return View(orderCreate);
        //}

        private async Task WriteToLog(OrderCreateViewModel orderCreate, string ex)
        {
            var dbLog = new DBLog
            {
                DateTime = orderCreate.Order?.OrderDate ?? DateTime.Now,
                Message = "Заказ не создан",
                User = User.Identity?.Name,
                Level = "ERROR",
                AdditionalInfo = ex
            };
            await _context.DBLog.AddAsync(dbLog);
            await _context.SaveChangesAsync();
            _logger.LogError(ex);
        }

        [HttpPost]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> RepeatOrder(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }
            await GetUserInfo();
            var order = await _context.Orders.Include(x => x.Items).FirstOrDefaultAsync(m => m.Id == id & m.UserId == _User.Id);

            if (order == null || order.Items == null || order.Items.Count == 0)
            {
                return RedirectToAction("index");
            }


            var userId = _User.Id;
            var today = DateTime.UtcNow.Date;
            var cacheKey = $"cart:{userId}:{today:yyyy-MM-dd}";

            // Удаляем корзину из Redis
            await _cache.RemoveAsync(cacheKey);
            List<ShoppingCart> cart = new List<ShoppingCart>();

            List<string> notAdded = new List<string>();

            //Проверка на то, существует ли товар в амо
            var repeatProductIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
            var productAmoIds = await _context.Products
                .Where(p => repeatProductIds.Contains(p.Id))
                .Select(p => new { p.Id, p.AmoCrmId })
                .ToDictionaryAsync(p => p.Id, p => p.AmoCrmId);

            foreach (var item in order.Items)
            {
                productAmoIds.TryGetValue(item.ProductId, out var amoId);
                item.AmoCrmId = amoId;
                if (item.AmoCrmId == null)
                {
                    notAdded.Add(item.ProductName);
                }
                else
                {
                    var cartItem = new ShoppingCart
                    {
                        Count = item.Count,
                        MeasureUnit = item.MeasureUnit,
                        OrderDate = DateTime.Now,
                        Price = item.Price,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        UserId = _User.Id,
                        AmoCrmId = item.AmoCrmId
                    };
                    cart.Add(cartItem);
                }
                               
            }
            if (notAdded.Count != 0)
            {
                TempData["NotAdded"] = JsonSerializer.Serialize(notAdded);
            }
            var jsonString = JsonSerializer.Serialize(cart);

            await _cache.SetStringAsync(cacheKey, jsonString, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
            });
            return RedirectToAction("Create");
        }


        private bool OrderExists(int id)
        {
          return _context.Orders.Any(e => e.Id == id);
        }


        async Task GetUserInfo()
        {
            try
            {
                var UserClime = User.FindFirst(ClaimTypes.NameIdentifier);

                // Resolve the user via their email
                if (UserClime != null)
                {
                    _User = await _userManager.FindByIdAsync(UserClime.Value);

                }
                // Get the roles for the user
                if (_User != null)
                {
                    _Roles = await _userManager.GetRolesAsync(_User);

                }

            }
            catch (Exception ex)
            {
                await DBConnector.DBLogs.Error($"Не удалось получить информацию по пользователю", ClaimTypes.NameIdentifier, $"{ex.Message}\n{ex.InnerException}");

            }

        }

    }
}
