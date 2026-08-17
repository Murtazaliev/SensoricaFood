using System.Collections.Generic;

namespace AVBDelivery.Models
{
    /// <summary>
    /// Примечание
    /// </summary>
    public class Note
    {
        public int Id { get; set; }

        public int AmoCrmId { get; set; }

        public string? Value { get; set; }

        public bool IsDeleted { get; set; }

        public ICollection<Organization> Organizations { get; set; } = new List<Organization>();
    }
}
