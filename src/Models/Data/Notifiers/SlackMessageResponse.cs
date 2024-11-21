using System.Text.Json.Serialization;

namespace Kurrent.Models.Data.Notifiers;

public class SlackMessageResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }
    
    [JsonPropertyName("error")]
    public string? Error { get; set; }
    
    [JsonPropertyName("errors")]
    public string[]? Errors { get; set; }
    
    [JsonPropertyName("response_metadata")]
    public ResponseMetadata? ResponseMetadata { get; set; }
}

public class ResponseMetadata
{
    [JsonPropertyName("messages")]
    public string[]? Messages { get; set; }
}