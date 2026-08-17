using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm.Requests
{

    public class Lead
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }
        [JsonPropertyName("price")]
        public int? Price { get; set; }

        [JsonPropertyName("pipeline_id")]
        public int? PipelineId { get; set; }

        [JsonPropertyName("created_by")]
        public int? CreatedBy { get; set; } = 0;
        [JsonPropertyName("_embedded")]
        public LeadEmbedded? Embedded { get; set; }
        [JsonPropertyName("custom_fields_values")]
        public CustomFieldValues[]? CustomFieldsValues { get; set; }

    }

    public class LeadEmbedded
    {
        [JsonPropertyName("catalog_elements")]
        public CatalogElement[] CatalogElements { get; set; }
        [JsonPropertyName("contacts")]
        public LeadContact[]? Contacts { get; set; }
        [JsonPropertyName("companies")]
        public LeadCompany[]? Companies { get; set; }
        
    }

    public class LeadContact
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }
    }
    public class LeadCompany
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }
    }
    public class CatalogElement
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("metadata")]
        public Metadata Metadata { get; set; }
    }

    public class Metadata
    {
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        [JsonPropertyName("catalog_id")]
        public string CatalogId { get; set; }
        [JsonPropertyName("price_id")]
        public int PriceId { get; set; }
    }

}
