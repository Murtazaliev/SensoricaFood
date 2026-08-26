using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AVBDelivery.Interfaces;
using AVBDelivery.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace AVBDelivery.Features.Orders.RepeatOrder
{
    public record RepeatOrderCommand(int? Id) : IRequest<RepeatOrderResult>;

    public class RepeatOrderResult
    {
        public bool Success { get; set; }
        public List<string>? NotAddedItems { get; set; }
    }

    public class RepeatOrderCommandHandler : IRequestHandler<RepeatOrderCommand, RepeatOrderResult>
    {
        private readonly ApplicationContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IDistributedCache _cache;

        public RepeatOrderCommandHandler(
            ApplicationContext context,
            ICurrentUserService currentUser,
            IDistributedCache cache)
        {
            _context = context;
            _currentUser = currentUser;
            _cache = cache;
        }

        public async Task<RepeatOrderResult> Handle(RepeatOrderCommand request, CancellationToken ct)
        {
            if (request.Id == null)
            {
                return new RepeatOrderResult { Success = false };
            }

            var user = await _currentUser.GetUserAsync();
            var order = await _context.Orders
                .Include(x => x.Items)
                .FirstOrDefaultAsync(m => m.Id == request.Id && m.UserId == user!.Id, ct);

            if (order == null || order.Items == null || order.Items.Count == 0)
            {
                return new RepeatOrderResult { Success = false };
            }

            var userId = user.Id;
            var today = DateTime.UtcNow.Date;
            var cacheKey = string.Format(OrderConstants.CacheKeys.Cart, userId, today);

            await _cache.RemoveAsync(cacheKey, ct);
            var cart = new List<ShoppingCart>();
            var notAdded = new List<string>();

            var repeatProductIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
            var productAmoIds = await _context.Products
                .Where(p => repeatProductIds.Contains(p.Id))
                .Select(p => new { p.Id, p.AmoCrmId })
                .ToDictionaryAsync(p => p.Id, p => p.AmoCrmId, ct);

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
                    cart.Add(new ShoppingCart
                    {
                        Count = item.Count,
                        MeasureUnit = item.MeasureUnit,
                        OrderDate = DateTime.Now,
                        Price = item.Price,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        UserId = user.Id,
                        AmoCrmId = item.AmoCrmId
                    });
                }
            }

            var jsonString = JsonSerializer.Serialize(cart);
            await _cache.SetStringAsync(cacheKey, jsonString, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = OrderConstants.CacheExpiration.CartTtl
            }, ct);

            return new RepeatOrderResult
            {
                Success = true,
                NotAddedItems = notAdded.Count > 0 ? notAdded : null
            };
        }
    }
}
