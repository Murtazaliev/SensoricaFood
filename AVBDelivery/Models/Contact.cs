using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Identity.Client;

namespace AVBDelivery.Models
{
    public class Contact
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string? Name { get; set; }
        public bool IsDeleted { get; set; } = false;
        //public string? OrganizationId { get; set; }
        public List<Organization>? Organizations { get; set; }
        public string? AmoCrmId { get; set; }
    }
}
