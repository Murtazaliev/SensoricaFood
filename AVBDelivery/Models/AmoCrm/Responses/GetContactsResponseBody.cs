using System.Text.Json.Serialization;
using AVBDelivery.Models.AmoCrm;

namespace AVBDelivery.Models.AmoCrm.Responses
{

    public class GetContactsResponseBody
    {
        [JsonPropertyName("_page")]
        public int Page { get; set; }
        [JsonPropertyName("_links")]
        public Links Links { get; set; }
        [JsonPropertyName("_embedded")]
        public Embedded Embedded { get; set; }
    }

    public class Embedded
    {
        [JsonPropertyName("contacts")]
        public AmoContact[] Contacts { get; set; }
    }



}
