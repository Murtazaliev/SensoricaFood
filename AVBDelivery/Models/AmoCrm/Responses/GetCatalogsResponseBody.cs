using AVBDelivery.Models.AmoCrm;
using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm.Responses
{

    public class GetCatalogsResponseBody
    {
        [JsonPropertyName("_page")]
        public int Page { get; set; }
        [JsonPropertyName("_links")]
        public Links Links { get; set; }
        [JsonPropertyName("_embedded")]
        public GetCatalogsEmbedded Embedded { get; set; }
    }

    public class GetCatalogsEmbedded
    {
        [JsonPropertyName("catalogs")]
        public Catalog[]? Catalogs { get; set; }
    }

    public class Catalog
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("created_by")]
        public int CreatedBy { get; set; }
        [JsonPropertyName("updated_by")]
        public int UpdatedBy { get; set; }
        [JsonPropertyName("created_at")]
        public int CreatedAt { get; set; }
        [JsonPropertyName("updated_at")]
        public int UpdatedAt { get; set; }
        [JsonPropertyName("sort")]
        public int Sort { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("can_add_elements")]
        public bool CanAddElements { get; set; }
        [JsonPropertyName("can_show_in_cards")]
        public bool CanShowInCards { get; set; }
        [JsonPropertyName("can_link_multiple")]
        public bool CanLinkMultiple { get; set; }
        [JsonPropertyName("can_be_deleted")]
        public bool CanBeDeleted { get; set; }
        [JsonPropertyName("sdk_widget_code")]
        public int? SdkWidgetCode { get; set; }
        [JsonPropertyName("account_id")]
        public int AccountId { get; set; }
        [JsonPropertyName("_links")]
        public Links Links { get; set; }
    }

}
