using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm
{
    public class Tag
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("color")]
        public int? Color { get; set; }
    }
}
