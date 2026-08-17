using System.Collections.Generic;
using AVBDelivery.Models;
using Org.BouncyCastle.Asn1.Cmp;

namespace AVBDelivery.ViewModels
{
    public class UsersViewModel
    {
        public List<UserWithClient>? UsersWithClients { get; set; }
    }
    public class UserWithClient
    {
        public User? User { get; set; }
        public Contact? Contact { get; set; }
    }
}
