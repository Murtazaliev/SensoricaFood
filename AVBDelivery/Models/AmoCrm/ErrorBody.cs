using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm
{
    public class ErrorBody
    {
        [JsonPropertyName("validationerrors")]
        public ValidationErrors[] ValidationErrors { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("status")]
        public int Status { get; set; }
        [JsonPropertyName("detail")]
        public string Detail { get; set; }
    }

    public class ValidationErrors
    {
        [JsonPropertyName("request_id")]
        public string Request_id { get; set; }
        [JsonPropertyName("errors")]
        public Error[] Errors { get; set; }
    }

    public class Error
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("path")]
        public string Path { get; set; }
        [JsonPropertyName("detail")]
        public string Detail { get; set; }
    }

}
