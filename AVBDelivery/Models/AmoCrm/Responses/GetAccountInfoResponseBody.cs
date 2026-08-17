using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm.Responses
{

    public class GetAccountInfoResponseBody
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("subdomain")]
        public string? Subdomain { get; set; }
        [JsonPropertyName("created_at")]
        public int CreatedAt { get; set; }
        [JsonPropertyName("created_by")]
        public int CreatedBy { get; set; }
        [JsonPropertyName("updated_at")]
        public int UpdatedAt { get; set; }
        [JsonPropertyName("updated_by")]
        public int UpdatedBy { get; set; }
        [JsonPropertyName("current_user_id")]
        public int CurrentUserId { get; set; }
        [JsonPropertyName("country")]
        public string? Country { get; set; }
        [JsonPropertyName("currency")]
        public string? Currency { get; set; }
        [JsonPropertyName("currency_symbol")]
        public string? CurrencySymbol { get; set; }
        [JsonPropertyName("customers_mode")]
        public string? CustomersMode { get; set; }
        [JsonPropertyName("is_unsorted_on")]
        public bool IsUnsortedOn { get; set; }
        [JsonPropertyName("mobile_feature_version")]
        public int MobileFeatureVersion { get; set; }
        [JsonPropertyName("is_loss_reason_enabled")]
        public bool IsLossReasonEnabled { get; set; }
        [JsonPropertyName("is_helpbot_enabled")]
        public bool IsHelpbotEnabled { get; set; }
        [JsonPropertyName("is_technical_account")]
        public bool IsTechnicalAccount { get; set; }
        [JsonPropertyName("contact_name_display_order")]
        public int ContactNameDisplayOrder { get; set; }
        [JsonPropertyName("drive_url")]
        public string? DriveUrl { get; set; }
        [JsonPropertyName("_links")]
        public Links? Links { get; set; }
    }

}
