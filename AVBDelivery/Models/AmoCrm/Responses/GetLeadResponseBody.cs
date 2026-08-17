using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm.Responses
{
    public sealed class GetLeadResponseBody
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("price")]
        public decimal? Price { get; set; }

        [JsonPropertyName("responsible_user_id")]
        public long? ResponsibleUserId { get; set; }

        [JsonPropertyName("group_id")]
        public long? GroupId { get; set; }

        [JsonPropertyName("status_id")]
        public long? StatusId { get; set; }

        [JsonPropertyName("pipeline_id")]
        public long? PipelineId { get; set; }

        [JsonPropertyName("loss_reason_id")]
        public long? LossReasonId { get; set; }

        [JsonPropertyName("source_id")]
        public long? SourceId { get; set; }

        [JsonPropertyName("created_by")]
        public long? CreatedBy { get; set; }

        [JsonPropertyName("updated_by")]
        public long? UpdatedBy { get; set; }

        // В API это unix time (seconds)
        [JsonPropertyName("created_at")]
        public long? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public long? UpdatedAt { get; set; }

        [JsonPropertyName("closed_at")]
        public long? ClosedAt { get; set; }

        [JsonPropertyName("closest_task_at")]
        public long? ClosestTaskAt { get; set; }

        [JsonPropertyName("is_deleted")]
        public bool IsDeleted { get; set; }

        [JsonPropertyName("custom_fields_values")]
        public CustomFieldValues[]? CustomFieldsValues { get; set; }

        [JsonPropertyName("score")]
        public int? Score { get; set; }

        [JsonPropertyName("account_id")]
        public long? AccountId { get; set; }

        [JsonPropertyName("is_price_modified_by_robot")]
        public bool IsPriceModifiedByRobot { get; set; }

        [JsonPropertyName("_links")]
        public Links? Links { get; set; }

        [JsonPropertyName("_embedded")]
        public GetLeadEmbedded? Embedded { get; set; }
    }

    public sealed class Links
    {
        [JsonPropertyName("self")]
        public LinkObject? Self { get; set; }
    }

    public sealed class LinkObject
    {
        [JsonPropertyName("href")]
        public string? Href { get; set; }
    }

    public sealed class GetLeadEmbedded
    {
        [JsonPropertyName("tags")]
        public List<Tag>? Tags { get; set; }

        [JsonPropertyName("catalog_elements")]
        public List<CatalogElement>? CatalogElements { get; set; }

        // В вашем JSON это массив из 1 элемента
        [JsonPropertyName("loss_reason")]
        public List<LossReason>? LossReason { get; set; }

        [JsonPropertyName("companies")]
        public List<CompanyRef>? Companies { get; set; }

        [JsonPropertyName("contacts")]
        public List<ContactRef>? Contacts { get; set; }

        [JsonPropertyName("source")]
        public SourceInfo? Source { get; set; }
    }

    public sealed class Tag
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("color")]
        public string? Color { get; set; }
    }

    public sealed class CatalogElement
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("metadata")]
        public CatalogMetadata? Metadata { get; set; }
    }

    public sealed class CatalogMetadata
    {
        [JsonPropertyName("quantity")]
        public int? Quantity { get; set; }

        [JsonPropertyName("catalog_id")]
        public long? CatalogId { get; set; }
    }

    public sealed class LossReason
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("sort")]
        public int? Sort { get; set; }

        [JsonPropertyName("created_at")]
        public long? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public long? UpdatedAt { get; set; }

        [JsonPropertyName("_links")]
        public Links? Links { get; set; }
    }

    public sealed class CompanyRef
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("_links")]
        public Links? Links { get; set; }
    }

    public sealed class ContactRef
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("is_main")]
        public bool? IsMain { get; set; }

        [JsonPropertyName("_links")]
        public Links? Links { get; set; }
    }

    public sealed class SourceInfo
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
