using AVBDelivery.Models.AmoCrm;
using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm.Responses
{

    public class CreateElementsResponseBody
    {
        [JsonPropertyName("_links")]
        public Links Links { get; set; }
        [JsonPropertyName("_embedded")]
        public CreateElementsEmbedded Embedded { get; set; }
    }
    public class CreateElementsEmbedded
    {
        [JsonPropertyName("elements")]
        public Element[] Elements { get; set; }
    }
}
