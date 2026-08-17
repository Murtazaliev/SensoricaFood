using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm.Responses
{
    public class CreateLeadResponseBody
    {
        [JsonPropertyName("_links")]
        public Links Links { get; set; }
        [JsonPropertyName("_embedded")]
        public CreateLeadEmbedded Embedded { get; set; }
    }

    public class CreateLeadEmbedded
    {
        [JsonPropertyName("leads")]
        public CreatedLead[] Leads { get; set; }
    }

    public class CreatedLead
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("request_id")]
        public string Request_id { get; set; }
        [JsonPropertyName("_links")]
        public Links Links { get; set; }
    }
}
