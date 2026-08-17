using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm.Responses
{

    public class GetCompaniesResponseBody
    {
        [JsonPropertyName("_page")]
        public int Page { get; set; }
        [JsonPropertyName("_links")]
        public Links Links { get; set; }
        [JsonPropertyName("_embedded")]
        public CompaniesEmbedded Embedded { get; set; }
    }

    public class CompaniesEmbedded
    {
        [JsonPropertyName("companies")]
        public Company[] Companies { get; set; }
    }
}
