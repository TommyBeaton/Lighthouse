using System.Text.Json.Serialization;

namespace Kurrent.Models.Data.API;

public class AcrListTagsResponse
{
    [JsonPropertyName("registry")]
    public string Registry { get; set; }

    [JsonPropertyName("imageName")]
    public string ImageName { get; set; }

    [JsonPropertyName("tags")]
    public Tag[] Tags { get; set; }
}

public class Tag
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("digest")]
    public string Digest { get; set; }

    [JsonPropertyName("createdTime")]
    public DateTimeOffset CreatedTime { get; set; }

    [JsonPropertyName("lastUpdateTime")]
    public DateTimeOffset LastUpdateTime { get; set; }

    [JsonPropertyName("signed")]
    public bool Signed { get; set; }

    [JsonPropertyName("quarantineState")]
    public string QuarantineState { get; set; }

    [JsonPropertyName("changeableAttributes")]
    public ChangeableAttributes ChangeableAttributes { get; set; }
}

public class ChangeableAttributes
{
    [JsonPropertyName("deleteEnabled")]
    public bool DeleteEnabled { get; set; }

    [JsonPropertyName("writeEnabled")]
    public bool WriteEnabled { get; set; }

    [JsonPropertyName("readEnabled")]
    public bool ReadEnabled { get; set; }

    [JsonPropertyName("listEnabled")]
    public bool ListEnabled { get; set; }
}
