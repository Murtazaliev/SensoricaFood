using AVBDelivery.Models.AmoCrm;
using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm.Requests
{
    public class ElementToUpdate
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("custom_fields_values")]
        public CustomFieldValues[] CustomFieldsValues { get; set; }
    }
}
