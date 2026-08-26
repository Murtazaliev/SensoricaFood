using AVBDelivery.Models;
using System;
using System.Collections.Generic;

namespace AVBDelivery.ViewModels
{
    public class OrderUploadPreviewViewModel
    {
        public List<OrderGroupByLocation> OrderGroups { get; set; } = new();
        public List<string> UnmatchedNames { get; set; } = new();
        public string SheetName { get; set; }
    }

    public class OrderGroupByLocation
    {
        public string ColumnHeader { get; set; }
        public string OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public bool OrganizationFound { get; set; }
        public List<MatchedOrderItem> Items { get; set; } = new();
        public int TotalQuantity => Items.FindAll(i => i.IsFound && i.Quantity > 0).Count;
    }

    public class MatchedOrderItem
    {
        public string FileName { get; set; }
        public string? ProductId { get; set; }
        public string? ProductName { get; set; }
        public float? Price { get; set; }
        public string? MeasureUnit { get; set; }
        public int? AmoCrmId { get; set; }
        public double Quantity { get; set; }
        public bool IsFound { get; set; }
    }
}
