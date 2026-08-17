using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm
{
    public class CustomFieldValues
    {
        [JsonPropertyName("field_id")]
        public int? FieldId { get; set; }
        [JsonPropertyName("field_name")]
        public string? FieldName { get; set; }
        [JsonPropertyName("field_code")]
        public string? FieldCode { get; set; }
        [JsonPropertyName("field_type")]
        public string? FieldType { get; set; }
        [JsonPropertyName("values")]
        public ElementValue[] Values { get; set; }
    }

    public class ElementValue
    {
        [JsonPropertyName("value")]
        public object? Value { get; set; }
        [JsonPropertyName("enum_id")]
        public int? EnumId { get; set; }
        [JsonPropertyName("enum_code")]
        public string? EnumCode { get; set; }
        
    }
    public class FileValue
    {
        [JsonPropertyName("file_uuid")]
        public string? FileUuid { get; set; }
        [JsonPropertyName("version_uuid")]
        public string? VersionUuid { get; set; }
        [JsonPropertyName("file_name")]
        public string? FileName { get; set; }
        [JsonPropertyName("file_size")]
        public long? FileSize { get; set; }
        [JsonPropertyName("is_deleted")]
        public bool? IsDeleted { get; set; }
    }
}
