using System.Collections.Generic;
using System.Threading.Tasks;
using AVBDelivery.Models;

namespace AVBDelivery.Interfaces
{
    public interface ICurrentUserService
    {
        Task<User?> GetUserAsync();
        Task<IList<string>> GetRolesAsync();
        string? UserId { get; }
    }
}
