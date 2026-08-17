using System.Collections.Generic;
using AVBDelivery.Models;

namespace AVBDelivery.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsClient { get; set; }
    }
}
