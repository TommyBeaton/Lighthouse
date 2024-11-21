namespace Kurrent.Utils;

public class AppConfig
{
    public List<WebhookConfig>? Webhooks { get; set; }
    public List<RepositoryConfig>? Repositories { get; set; }
    public List<PollerConfig>? Pollers { get; set; }
    public List<NotifierConfig>? Notifiers { get; set; }
}

public class WebhookConfig
{
    public string EventName { get; set; }
    public string Path { get; set; }
    public string Type { get; set; }
}

public class RepositoryConfig
{
    public string Name { get; set; }
    public string Url { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public List<string> FileExtensions { get; set; } = new(); // Example: [".yaml", ".yml", ".txt"]
    public List<string> EventSubscriptions { get; set; } = new();
    public string Branch { get; set; }
}

public class PollerConfig
{
    public string EventName { get; set; }
    public string Type { get; set; }
    public int IntervalInSeconds { get; set; }
    public string? Url { get; set; }
    public string[] Images { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }

    public override string ToString()
    {
        return $"{nameof(EventName)}: {EventName}, {nameof(Type)}: {Type}, {nameof(IntervalInSeconds)}: {IntervalInSeconds}, {nameof(Url)}: {Url}, {nameof(Images)}: {Images}";
    }
}

public class NotifierConfig
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Token { get; set; }
    public string Channel { get; set; }
    public List<string> EventSubscriptions { get; set; } = new();
}

public class System
{
    public string Name { get; set; }
}