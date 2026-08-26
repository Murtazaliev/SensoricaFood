using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AVBDelivery.Interfaces;
using AVBDelivery.Models;
using AVBDelivery.ViewModels;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace AVBDelivery.Features.Orders.GetCreateOrderData
{
    public record GetCreateOrderDataQuery : IRequest<OrderCreateViewModel>;

    public class GetCreateOrderDataQueryHandler : IRequestHandler<GetCreateOrderDataQuery, OrderCreateViewModel>
    {
        private readonly ApplicationContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IDistributedCache _cache;

        public GetCreateOrderDataQueryHandler(
            ApplicationContext context,
            ICurrentUserService currentUser,
            IDistributedCache cache)
        {
            _context = context;
            _currentUser = currentUser;
            _cache = cache;
        }

        public async Task<OrderCreateViewModel> Handle(GetCreateOrderDataQuery request, CancellationToken ct)
        {
            var user = await _currentUser.GetUserAsync();

            var order = new Order
            {
                Items = new List<OrderItem>(),
                Sum = 0
            };

            var today = DateTime.UtcNow.Date;
            var cacheKey = string.Format(OrderConstants.CacheKeys.Cart, user!.Id, today);
            var cachedJson = await _cache.GetStringAsync(cacheKey, ct);

            if (!string.IsNullOrEmpty(cachedJson))
            {
                var shoppingCart = JsonSerializer.Deserialize<List<ShoppingCart>>(cachedJson);

                foreach (var itemGroup in shoppingCart!.GroupBy(p => p.ProductId))
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
                .Where(o => o.Contacts.Any(c => c.UserId == user.Id))
                .ToListAsync(ct);

            var todayDate = DateOnly.FromDateTime(DateTime.Today);
            var lastDeliveryDate = todayDate.AddDays(7);
            var dates = new List<DateOnly>();
            for (var date = todayDate.AddDays(1); date <= lastDeliveryDate; date = date.AddDays(1))
                dates.Add(date);

            return new OrderCreateViewModel
            {
                Dates = dates,
                Order = order,
                Organizations = organizations
            };
        }
    }
}
