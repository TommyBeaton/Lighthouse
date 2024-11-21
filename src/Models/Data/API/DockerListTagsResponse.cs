using System.Text.Json.Serialization;

namespace Kurrent.Models.Data.API;

public class DockerListTagsResponse
{
    [JsonPropertyName("count")]
    public long Count { get; set; }

    [JsonPropertyName("next")]
    public Uri Next { get; set; }

    [JsonPropertyName("previous")]
    public object Previous { get; set; }

    [JsonPropertyName("results")]
    public Result[] Results { get; set; }
}

public class Result
{
    [JsonPropertyName("creator")]
    public long Creator { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("images")]
    public DockerImage[] Images { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTimeOffset LastUpdated { get; set; }

    [JsonPropertyName("last_updater")]
    public long LastUpdater { get; set; }

    [JsonPropertyName("last_updater_username")]
    public string LastUpdaterUsername { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("repository")]
    public long Repository { get; set; }

    [JsonPropertyName("full_size")]
    public long FullSize { get; set; }

    [JsonPropertyName("v2")]
    public bool V2 { get; set; }

    [JsonPropertyName("tag_status")]
    public string TagStatus { get; set; }

    [JsonPropertyName("tag_last_pulled")]
    public DateTimeOffset TagLastPulled { get; set; }

    [JsonPropertyName("tag_last_pushed")]
    public DateTimeOffset TagLastPushed { get; set; }

    [JsonPropertyName("media_type")]
    public string MediaType { get; set; }

    [JsonPropertyName("content_type")]
    public string ContentType { get; set; }

    [JsonPropertyName("digest")]
    public string Digest { get; set; }
}

public class DockerImage
{
    [JsonPropertyName("architecture")]
    public string Architecture { get; set; }

    [JsonPropertyName("features")]
    public string Features { get; set; }

    [JsonPropertyName("variant")]
    public string? Variant { get; set; }

    [JsonPropertyName("digest")]
    public string Digest { get; set; }

    [JsonPropertyName("os")]
    public string OS { get; set; }

    [JsonPropertyName("os_features")]
    public string OsFeatures { get; set; }

    [JsonPropertyName("os_version")]
    public object OsVersion { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("last_pulled")]
    public DateTimeOffset LastPulled { get; set; }

    [JsonPropertyName("last_pushed")]
    public DateTimeOffset LastPushed { get; set; }
}