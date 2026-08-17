using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm.Responses
{
    public class GetFileByUuidResponseBody
    {
        [JsonPropertyName("_links")]
        public FileLinks Links { get; set; }
        [JsonPropertyName("created_at")]
        public int CreatedAt { get; set; }
        [JsonPropertyName("created_by")]
        public Created_By CreatedBy { get; set; }
        [JsonPropertyName("deleted_at")]
        public int? DeletedAt { get; set; }
        [JsonPropertyName("deleted_by")]
        public Deleted_By? DeletedBy { get; set; }
        [JsonPropertyName("has_multiple_versions")]
        public bool HasMultipleVersions { get; set; }
        [JsonPropertyName("id")]
        public long Id { get; set; }
        [JsonPropertyName("is_trashed")]
        public bool IsTrashed { get; set; }
        [JsonPropertyName("metadata")]
        public FileMetadata Metadata { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("previews")]
        public object? Previews { get; set; }
        [JsonPropertyName("sanitized_name")]
        public string SanitizedName { get; set; }
        [JsonPropertyName("size")]
        public int Size { get; set; }
        [JsonPropertyName("source_id")]
        public string SourceId { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("updated_at")]
        public int UpdatedAt { get; set; }
        [JsonPropertyName("updated_by")]
        public Updated_By? UpdatedBy { get; set; }
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }
        [JsonPropertyName("version_uuid")]
        public string VersionUuid { get; set; }
    }

    public class FileLinks
    {
        [JsonPropertyName("download")]
        public Download? Download { get; set; }
        [JsonPropertyName("download_version")]
        public Download_Version? DownloadVersion { get; set; }
        [JsonPropertyName("self")]
        public Self? Self { get; set; }
    }

    public class Download
    {
        [JsonPropertyName("href")]
        public string Href { get; set; }
    }

    public class Download_Version
    {
        [JsonPropertyName("href")]
        public string Href { get; set; }
    }

    public class Self
    {
        [JsonPropertyName("href")]
        public string Href { get; set; }
    }

    public class Created_By
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    public class FileMetadata
    {
        [JsonPropertyName("extension")]
        public string Extension { get; set; }
        [JsonPropertyName("mime_type")]
        public string MimeType { get; set; }
    }

    public class Updated_By
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }
    public class Deleted_By
    {
        [JsonPropertyName("type")]
        public string type { get; set; }
        [JsonPropertyName("id")]
        public int id { get; set; }
    }
}
