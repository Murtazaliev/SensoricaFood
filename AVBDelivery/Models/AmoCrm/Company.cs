using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm
{
    public class Company
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("responsible_user_id")]
        public int? ResponsibleUserId { get; set; }
        [JsonPropertyName("group_id")]
        public int? GroupId { get; set; }
        [JsonPropertyName("created_by")]
        public int? CreatedBy { get; set; }
        [JsonPropertyName("updated_by")]
        public int? UpdatedBy { get; set; }
        [JsonPropertyName("created_at")]
        public int? CreatedAt { get; set; }
        [JsonPropertyName("updated_at")]
        public int? UpdatedAt { get; set; }
        [JsonPropertyName("closest_task_at")]
        public int? ClosestTaskAt { get; set; }
        [JsonPropertyName("is_deleted")]
        public bool? IsDeleted { get; set; }
        [JsonPropertyName("custom_fields_values")]
        public CustomFieldValues[]? CustomFieldsValues { get; set; }
        [JsonPropertyName("account_id")]
        public int? AccountId { get; set; }
        [JsonPropertyName("_links")]
        public Links? Links { get; set; }
        [JsonPropertyName("_embedded")]
        public CompanyEmbedded? Embedded { get; set; }
    }

    public class CompanyEmbedded
    {
        [JsonPropertyName("tags")]
        public Tag[]? tags { get; set; }
    }
}
