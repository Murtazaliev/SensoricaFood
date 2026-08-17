using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm
{
    public class Link
    {
        [JsonPropertyName("to_entity_id")]
        public int ToEntityId { get; set; }
        [JsonPropertyName("to_entity_type")]
        public string ToEntityType { get; set; }
        [JsonPropertyName("metadata")]
        public LinkMetadata Metadata { get; set; }
    }

    public class LinkMetadata
    {
        [JsonPropertyName("quantity")]
        public float? Quantity { get; set; }
        [JsonPropertyName("catalog_id")]
        public int Catalog_id { get; set; }
    }

}
