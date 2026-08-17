using System.Text.Json.Serialization;
using AVBDelivery.Models.AmoCrm;

namespace AVBDelivery.Models.AmoCrm.Requests
{
    public class ElementToCreate
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("custom_fields_values")]
        public CustomFieldValues[] CustomFieldsValues { get; set; }
    }
}
