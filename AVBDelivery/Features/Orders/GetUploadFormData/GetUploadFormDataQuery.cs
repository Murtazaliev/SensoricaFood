using System.Threading;
using System.Threading.Tasks;
using AVBDelivery.ViewModels;
using MediatR;

namespace AVBDelivery.Features.Orders.GetUploadFormData
{
    public record GetUploadFormDataQuery : IRequest<OrderUploadPreviewViewModel>;

    public class GetUploadFormDataQueryHandler : IRequestHandler<GetUploadFormDataQuery, OrderUploadPreviewViewModel>
    {
        public Task<OrderUploadPreviewViewModel> Handle(GetUploadFormDataQuery request, CancellationToken ct)
        {
            return Task.FromResult(new OrderUploadPreviewViewModel());
        }
    }
}
