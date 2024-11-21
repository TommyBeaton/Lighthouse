using System.Text.Json.Serialization;

namespace Kurrent.Models.Data.Notifiers;

public class SlackMessageRequest
{
    [JsonPropertyName("channel")]
    public string Channel { get; set; }
    
    [JsonPropertyName("attachments")]
    public List<Attachment> Attachments { get; set; }
}

public class Attachment
{
    [JsonPropertyName("color")]
    public string Color { get; set; }

    [JsonPropertyName("blocks")]
    public List<Block> Blocks { get; set; }
}

public class Block
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("text")]
    public Text? Text { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("elements")]
    public List<Element>? Elements { get; set; }
}

public class Element
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }
}

public class Text
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("text")] // Unfortunately the inner name is also text so there's a slight naming conflict here. Using content instead of TextText 🔥
    public string Content { get; set; }
}