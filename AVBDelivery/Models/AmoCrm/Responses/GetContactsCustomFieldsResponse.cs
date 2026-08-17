using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace AVBDelivery.Models.AmoCrm.Responses
{

    public class GetContactsCustomFieldsResponseBody
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
        public ContactsCustomFieldsEmbedded Embedded { get; set; }
    }

    public class ContactsCustomFieldsEmbedded
    {
        [JsonPropertyName("custom_fields")]
        public ContactsCustomField[] CustomFields { get; set; }
    }

    public class ContactsCustomField
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("account_id")]
        public int Account_id { get; set; }
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("sort")]
        public int Sort { get; set; }
        [JsonPropertyName("is_api_only")]
        public bool IsApiOnly { get; set; }
        [JsonPropertyName("enums")]
        public ContactsCustomFieldsEnum[]? Enums { get; set; }
        [JsonPropertyName("group_id")]
        public int? GroupId { get; set; }
        [JsonPropertyName("required_statuses")]
        public Status[]? RequiredStatuses { get; set; }
        [JsonPropertyName("is_deletable")]
        public bool? IsDeletable { get; set; }
        [JsonPropertyName("is_predefined")]
        public bool? IsPredefined { get; set; }
        [JsonPropertyName("entity_type")]
        public string? EntityType { get; set; }
        [JsonPropertyName("remind")]
        public string? Remind { get; set; }
        [JsonPropertyName("triggers")]
        public string[]? Triggers { get; set; }
        [JsonPropertyName("currency")]
        public string? Currency { get; set; }
        [JsonPropertyName("hidden_statuses")]
        public Status[]? HiddenStatuses { get; set; }
        [JsonPropertyName("_links")]
        public Links? Links { get; set; }
    }

    public class ContactsCustomFieldsEnum
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }
        [JsonPropertyName("value")]
        public string? Value { get; set; }
        [JsonPropertyName("sort")]
        public int? Sort { get; set; }
    }
    public class Status
    {
        [JsonPropertyName("status_id")]
        public int? StatusId { get; set; }
        [JsonPropertyName("pipeline_id")]
        public int? PipelineId { get; set; }
    }
}
