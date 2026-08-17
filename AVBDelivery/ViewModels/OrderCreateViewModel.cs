using AVBDelivery.Models;
using System;
using System.Collections.Generic;

namespace AVBDelivery.ViewModels
{
    public class OrderCreateViewModel
    {
        public Order? Order { get; set; }
        public List<Organization>? Organizations { get; set; }
        public List<DateOnly>? Dates { get; set; }
        public string? UserId { get; set; }
        public int? AmoContactId { get; set; }
        public int? AmoCompanyId { get; set; }

        public List<string> NotAddedItems { get; set; } = new();
    }
}
