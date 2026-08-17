using System.Collections.Generic;
using AVBDelivery.Models;

namespace AVBDelivery.ViewModels
{
    public class OrganizationsViewModel
    {
        public string? UserId { get; set; }
        public List<Organization>? Organizations { get; set; }
    }
}
