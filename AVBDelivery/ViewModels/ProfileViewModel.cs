using System.Collections.Generic;
using AVBDelivery.Models;

namespace AVBDelivery.ViewModels
{
    public class ProfileViewModel
    {
        public Contact? Contact { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }
}
