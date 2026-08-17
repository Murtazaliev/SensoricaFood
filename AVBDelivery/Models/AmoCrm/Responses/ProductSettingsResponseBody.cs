using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm.Responses
{
    public class ProductSettingsResponseBody
    {
        [JsonPropertyName("is_enabled")]
        public bool IsEnabled { get; set; }

        [JsonPropertyName("catalog_id")]
        public int CatalogId { get; set; }
        [JsonPropertyName("_links")]
        public ProductLinks Links { get; set; }
    }

    public class ProductLinks
    {
        [JsonPropertyName("self")]
        public ProductSelf Self { get; set; }
    }

    public class ProductSelf
    {
        [JsonPropertyName("href")]
        public string Href { get; set; }
        [JsonPropertyName("method")]
        public string Method { get; set; }
    }

}
