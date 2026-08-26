using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AVBDelivery.Interfaces;
using AVBDelivery.Models;
using AVBDelivery.ViewModels;
using LinqKit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AVBDelivery.Features.Orders.GetOrders
{
    public record GetOrdersQuery(string? StartDate, string? EndDate, string? OrganizationId, int Page = 1)
        : IRequest<OrdersViewModel>;

    public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, OrdersViewModel>
    {
        private readonly ApplicationContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetOrdersQueryHandler(ApplicationContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<OrdersViewModel> Handle(GetOrdersQuery request, CancellationToken ct)
        {
            var user = await _currentUser.GetUserAsync();
            var roles = await _currentUser.GetRolesAsync();
            int pageSize = 20;
            int page = request.Page < 1 ? 1 : request.Page;

            var organizations = roles.Contains(OrderConstants.Roles.Admin)
                ? await _context.Organizations.ToListAsync(ct)
                : await _context.Organizations
                    .Where(o => o.Contacts.Any(c => c.UserId == user!.Id))
                    .ToListAsync(ct);

            Expression<Func<Order, bool>> predicate = p => p.UserId == user!.Id;

            if (request.StartDate != null)
            {
                var dtStartDate = System.Convert.ToDateTime(request.StartDate);
                predicate = predicate.And(x => x.OrderDate >= dtStartDate);
            }
            if (request.EndDate != null)
            {
                var dtEndDate = System.Convert.ToDateTime(request.EndDate);
                predicate = predicate.And(x => x.OrderDate <= dtEndDate.AddDays(1));
            }
            if (request.OrganizationId != null)
            {
                predicate = predicate.And(x => x.OrganizationId == request.OrganizationId);
            }

            var totalOrders = await _context.Orders.Where(predicate).CountAsync(ct);

            var orders = await _context.Orders
                .Where(predicate)
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .AsNoTracking()
                .ToListAsync(ct);

            return new OrdersViewModel
            {
                Organizations = organizations,
                Orders = orders,
                PageInfo = new PageInfo
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalItems = totalOrders
                }
            };
        }
    }
}
