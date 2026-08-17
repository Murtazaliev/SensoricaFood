using System.Threading.Tasks;
using AVBDelivery.Models.Responses;

namespace AVBDelivery.Interfaces
{
    public interface INomenclatureUploader
    {
        public Task<string> Start();
    }
}
