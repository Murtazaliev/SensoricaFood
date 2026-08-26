using System;
using System.Threading;
using System.Threading.Tasks;
using AVBDelivery.Interfaces;
using AVBDelivery.Models;
using AVBDelivery.ViewModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AVBDelivery.Features.Orders.GetOrderDetails
{
    public record GetOrderDetailsQuery(int? Id) : IRequest<OrderViewModel?>;

    public class GetOrderDetailsQueryHandler : IRequestHandler<GetOrderDetailsQuery, OrderViewModel?>
    {
        private readonly ApplicationContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetOrderDetailsQueryHandler(ApplicationContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<OrderViewModel?> Handle(GetOrderDetailsQuery request, CancellationToken ct)
        {
            if (request.Id == null) return null;

            var user = await _currentUser.GetUserAsync();
            var order = await _context.Orders
                .Include(x => x.Items)
                .FirstOrDefaultAsync(m => m.Id == request.Id && m.UserId == user!.Id, ct);

            if (order == null) return null;

            var address = (await _context.Organizations.FirstOrDefaultAsync(
                x => x.OrganizationId == order.OrganizationId, ct))?.DeliveryAddress;

            return new OrderViewModel
            {
                Order = order,
                Address = address
            };
        }
    }
}
