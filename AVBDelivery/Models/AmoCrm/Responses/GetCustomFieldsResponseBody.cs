using System.Text.Json.Serialization;
using AVBDelivery.Models.AmoCrm;

namespace AVBDelivery.Models.AmoCrm.Responses
{

    public class GetCustomFieldsResponseBody
    {
        [JsonPropertyName("_total_items")]
        public int TotalItems { get; set; }
        [JsonPropertyName("_page")]
        public int Page { get; set; }
        [JsonPropertyName("_page_count")]
        public int PageCount { get; set; }
        [JsonPropertyName("_links")]
        public Links Links { get; set; }
        [JsonPropertyName("_embedded")]
        public CustomFieldsEmbedded Embedded { get; set; }
    }

    public class CustomFieldsEmbedded
    {
        [JsonPropertyName("custom_fields")]
        public CustomField[] CustomFields { get; set; }
    }

   

}
