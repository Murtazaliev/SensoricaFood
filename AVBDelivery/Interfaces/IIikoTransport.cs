using AVBDelivery.Models.Responses;
using System.Threading.Tasks;

namespace AVBDelivery.Interfaces
{
    public interface IIikoTransport
    {
        public Task<string> GetAccessTokenAsync();
        public Task<OrganizationsResponseBody> GetOrganizationsAsync();
        public Task<ExternalMenusResponseBody> GetExternalMenusAsync();
        public Task<ExternalMenuByIdResponseBody> GetExternalMenuByIdAsync(string externalMenuId, string priceCategoryId, string[] organizationIds);
    }
}
