using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AVBDelivery.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AVBDelivery.Features.Orders.GetOrdersReport
{
    public record GetOrdersReportQuery : IRequest<List<Order>>;

    public class GetOrdersReportQueryHandler : IRequestHandler<GetOrdersReportQuery, List<Order>>
    {
        private readonly ApplicationContext _context;

        public GetOrdersReportQueryHandler(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> Handle(GetOrdersReportQuery request, CancellationToken ct)
        {
            var orders = await _context.Orders
                .Include(x => x.Items)
                .Where(x => x.OrderDate >= DateTime.Today && x.OrderDate < DateTime.Today.AddDays(1))
                .AsNoTracking()
                .ToListAsync(ct);

            var userIds = orders.Select(o => o.UserId).Distinct().ToList();
            var userNames = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName, ct);

            foreach (var item in orders)
            {
                if (userNames.TryGetValue(item.UserId, out var userName))
                {
                    item.UserId = userName;
                }
            }
            return orders;
        }
    }
}
