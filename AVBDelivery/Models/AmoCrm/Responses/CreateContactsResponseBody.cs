using System.Text.Json.Serialization;
using AVBDelivery.Models.AmoCrm;

namespace AVBDelivery.Models.AmoCrm.Responses
{

    public class CreateContactsResponseBody
    {
        [JsonPropertyName("_links")]
        public Links Links { get; set; }
        [JsonPropertyName("_embedded")]
        public CreateContactsEmbedded Embedded { get; set; }
    }
    public class CreateContactsEmbedded
    {
        [JsonPropertyName("contacts")]
        public CreatedContact[] CreatedContacts { get; set; }
    }

    public class CreatedContact
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("is_deleted")]
        public bool IsDeleted { get; set; }
        [JsonPropertyName("is_unsorted")]
        public bool IsUnsorted { get; set; }
        [JsonPropertyName("request_id")]
        public string RequestId { get; set; }
        [JsonPropertyName("_links")]
        public Links Links { get; set; }
    }

}
