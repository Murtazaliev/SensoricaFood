using System.Collections.Generic;
using AVBDelivery.Models;

namespace AVBDelivery.ViewModels
{
    public class OrdersViewModel : PageViewModel
    {
        public List<Order>? Orders { get; set; }
        public List<Organization>? Organizations { get; set; }
    }
}
