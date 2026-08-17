using System.Text.Json.Serialization;
using AVBDelivery.Models.AmoCrm;

namespace AVBDelivery.Models.AmoCrm.Requests
{
    public class CreateContactRequestBody
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }
        [JsonPropertyName("second_name")]
        public string? SecondName { get; set; }
        [JsonPropertyName("custom_fields_values")]
        public CustomFieldValues[]? CustomFieldsValues { get; set; }
    }
}
