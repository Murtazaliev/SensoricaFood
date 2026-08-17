using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm.Responses
{
    public class GetLeadPipelinesResponseBody
    {

        [JsonPropertyName("_total_items")]
        public int TotalItems { get; set; }

        [JsonPropertyName("_links")]
        public Links Links { get; set; }

        [JsonPropertyName("_embedded")]
        public GetPipelinesEmbedded Embedded { get; set; }
    }

    public class GetPipelinesEmbedded
    {
        [JsonPropertyName("pipelines")]
        public Pipeline[] Pipelines { get; set; }
    }

    public class Pipeline
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("sort")]
        public int Sort { get; set; }

        [JsonPropertyName("is_main")]
        public bool IsMain { get; set; }

        [JsonPropertyName("is_unsorted_on")]
        public bool IsUnsortedOn { get; set; }

        [JsonPropertyName("is_archive")]
        public bool IsArchive { get; set; }

        [JsonPropertyName("account_id")]
        public int AccountId { get; set; }

        [JsonPropertyName("_links")]
        public Links Links { get; set; }

        [JsonPropertyName("_embedded")]
        public PipelineEmbedded Embedded { get; set; }

    }

    public class PipelineEmbedded
    {
        [JsonPropertyName("statuses")]
        public PipelineStatus[] Statuses { get; set; }
    }

    public class PipelineStatus
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("sort")]
        public int Sort { get; set; }

        [JsonPropertyName("is_editable")]
        public bool IsEditable { get; set; }

        [JsonPropertyName("pipeline_id")]
        public int PipelineId { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("account_id")]
        public int AccountId { get; set; }

        [JsonPropertyName("links")]
        public Links Links { get; set; }
    }

}
