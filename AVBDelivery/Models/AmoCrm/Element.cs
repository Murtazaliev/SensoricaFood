using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm
{
    public class Element
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("created_by")]
        public int? CreatedBy { get; set; }
        [JsonPropertyName("updated_by")]
        public int? UpdatedBy { get; set; }
        [JsonPropertyName("created_at")]
        public int? CreatedAt { get; set; }
        [JsonPropertyName("updated_at")]
        public int? UpdatedAt { get; set; }
        [JsonPropertyName("is_deleted")]
        public bool? IsDeleted { get; set; }
        [JsonPropertyName("custom_fields_values")]
        public CustomFieldValues[]? CustomFieldsValues { get; set; }
        [JsonPropertyName("catalog_id")]
        public int? CatalogId { get; set; }
        [JsonPropertyName("account_id")]
        public int? AccountId { get; set; }
        [JsonPropertyName("_links")]
        public Links? Links { get; set; }
        [JsonPropertyName("_embedded")]
        public Embedded? Embedded { get; set; }
    }

    public class Embedded
    {
        [JsonPropertyName("warning")]
        public Warning? Warning { get; set; }
    }

    public class Warning
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
