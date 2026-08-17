using System;
using System.Collections.Generic;

namespace AVBDelivery.Models
{
    public class Nomenclature
    {
        public int Id { get; set; }
        public List<ProductGroup> ProductGroup { get; set; }
        public List<Product> Products { get; set; }
    }

    public class ProductGroup
    {
        public List<Product> Products { get; set; }
        public string Id { get; set; }
        public string GroupName { get; set; }
    }

    public class Product
    {
        public byte[]? Image { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public float Price { get; set; }
        public int Type { get; set; }
        public bool IsActive { get; set; }
        public string ParentGroupName { get; set; }
        public bool ProductInBlackList { get; set; }
        public string MeasureUnit { get; set; }
        public string Sku { get; set; }
        public string? Description { get; set; }
        public int? AmoCrmId { get; set; }
        public string? FullEnergy { get; set; }
        public float? PortionGram { get; set; }
    }
}



