using AVBDelivery.Models;
using System.Collections.Generic;

namespace AVBDelivery.ViewModels
{
    public class ProductListViewModel
    {
        public Nomenclature NomenclatureList { get; set; }

        public Order Order { get; set; }

        public SiteAnnouncement? SiteAnnouncement { get; set; }

        public Dictionary<string, List<int>> ProductImageIndexes { get; set; } = new();
    }
}
