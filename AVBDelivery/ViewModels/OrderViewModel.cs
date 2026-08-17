using AVBDelivery.Models;

namespace AVBDelivery.ViewModels
{
    public class OrderViewModel
    {
        public Order Order { get; set; }
        public string? Address { get; set; }

        public string? Link { get; set; }
    }
}
