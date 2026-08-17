using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm.Responses
{
    public class GetLeadLinksResponseBody
    {
        [JsonPropertyName("_links")]
        public Links Links { get; set; }
        [JsonPropertyName("_embedded")]
        public GetLinksEmbedded Embedded { get; set; }
    }

    public class GetLinksEmbedded
    {
        [JsonPropertyName("links")]
        public GetLink[] Links { get; set; }
    }

    public class GetLink
    {
        [JsonPropertyName("to_entity_id")]
        public int ToEntityId { get; set; }
        [JsonPropertyName("to_entity_type")]
        public string ToEntityType { get; set; }
        [JsonPropertyName("metadata")]
        public Metadata Metadata { get; set; }
    }

    public class Metadata
    {
        [JsonPropertyName("quantity")]
        public float? Quantity { get; set; }
        [JsonPropertyName("catalog_id")]
        public int? CatalogId { get; set; }
        [JsonPropertyName("price_id")]
        public int? PriceId { get; set; }
        [JsonPropertyName("main_contact")]
        public bool? MainContact { get; set; }
    }

}
