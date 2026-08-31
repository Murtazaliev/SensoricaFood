using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AVBDelivery.Interfaces;
using AVBDelivery.Jobs;
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
        private readonly AmoCrm _amoCrm;

        public GetOrderDetailsQueryHandler(ApplicationContext context, ICurrentUserService currentUser, AmoCrm amoCrm)
        {
            _context = context;
            _currentUser = currentUser;
            _amoCrm = amoCrm;
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

            string? link = null;
            if (order.AmoCrmId != null)
            {
                var createdLead = await _amoCrm.GetLead(order.AmoCrmId.Value);
                var base1c = createdLead?.CustomFieldsValues?.FirstOrDefault(f => f.FieldId == OrderConstants.AmoCrm.InvoiceLinkFieldId);
                link = base1c?.Values.FirstOrDefault()?.Value?.ToString();
            }

            return new OrderViewModel
            {
                Order = order,
                Address = address,
                Link = link
            };
        }
    }
}
