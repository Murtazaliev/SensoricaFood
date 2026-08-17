using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm.Responses
{
    public class CreateLeadLinkResponseBody
    {
        [JsonPropertyName("_links")]
        public Links Links { get; set; }
        [JsonPropertyName("_embedded")]
        public CreateLinkEmbedded Embedded { get; set; }
    }

    public class CreateLinkEmbedded
    {
        [JsonPropertyName("links")]
        public Link[] Links { get; set; }
    }
}
