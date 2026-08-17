using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm.Responses
{
    public class CreateCompaniesResponseBody
    {
        [JsonPropertyName("_links")]
        public Links Links { get; set; }
        [JsonPropertyName("_embedded")]
        public CreateCompaniesEmbedded Embedded { get; set; }
    }

    public class CreateCompaniesEmbedded
    {
        [JsonPropertyName("companies")]
        public CreatedCompany[] Companies { get; set; }
    }

    public class CreatedCompany
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("is_deleted")]
        public bool IsDeleted { get; set; }
        [JsonPropertyName("request_id")]
        public string RequestId { get; set; }
        [JsonPropertyName("_links")]
        public Links Links { get; set; }
    }

}
