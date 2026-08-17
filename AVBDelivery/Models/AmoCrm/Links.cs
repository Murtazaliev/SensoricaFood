using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm
{
    public class Links
    {
        [JsonPropertyName("self")]
        public Self Self { get; set; }
    }
    public class Self
    {
        [JsonPropertyName("href")]
        public string Href { get; set; }
    }
}
