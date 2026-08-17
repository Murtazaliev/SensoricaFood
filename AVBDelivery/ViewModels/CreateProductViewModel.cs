using AVBDelivery.Models;
using System.Collections.Generic;

namespace AVBDelivery.ViewModels
{
    public class CreateProductViewModel
    {
        public Dictionary<string, string> Groups { get; set; }
        public Product Product { get; set; }
    }
}
