using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm
{
    public class CustomFieldValues2
    {
        [JsonPropertyName("field_id")]
        public int FieldId { get; set; }
        [JsonPropertyName("field_name")]
        public string? FieldName { get; set; }
        [JsonPropertyName("field_code")]
        public string? FieldCode { get; set; }
        [JsonPropertyName("field_type")]
        public string? FieldType { get; set; }
        [JsonPropertyName("values")]
        public ElementValue2[] Values { get; set; }
    }

    public class ElementValue2
    {
        [JsonPropertyName("value")]
        public long? Value { get; set; }
        [JsonPropertyName("enum_id ")]
        public int? EnumId { get; set; }
        [JsonPropertyName("enum_code")]
        public string? EnumCode { get; set; }
    }
}
