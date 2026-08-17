using System.Text.Json.Serialization;
using AVBDelivery.Models.AmoCrm;

namespace AVBDelivery.Models.AmoCrm.Responses
{

    public class GetElementsResponseBody
    {
        [JsonPropertyName("_page")]
        public int Page { get; set; }
        [JsonPropertyName("_links")]
        public Links Links { get; set; }
        [JsonPropertyName("_embedded")]
        public ElementsEmbedded Embedded { get; set; }
    }

    public class ElementsEmbedded
    {
        [JsonPropertyName("elements")]
        public Element[] Elements { get; set; }
    }
}
