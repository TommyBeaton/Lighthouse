namespace Kurrent.Models.Data.Notifiers;

public class SlackAuthResponse
{
    public bool Ok { get; set; }
    public string Url { get; set; }
    public string Team { get; set; }
    public string User { get; set; }
    public string TeamId { get; set; }
    public string UserId { get; set; }
    public string Error { get; set; }
}