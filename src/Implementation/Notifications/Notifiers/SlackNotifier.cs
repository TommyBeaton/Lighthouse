using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Kurrent.Models.Data;
using Kurrent.Models.Data.Notifiers;
using Kurrent.Utils;

namespace Kurrent.Implementation.Notifications.Notifiers;

public class SlackNotifier : BaseNotifier
{
    private const string Uri = "https://slack.com/api/chat.postMessage";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SlackNotifier> _logger;

    public SlackNotifier(
        IHttpClientFactory httpClientFactory, 
        ILogger<SlackNotifier> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public override async Task<NotifierResult> NotifyAsync(
        Image image, 
        RepositoryConfig repositoryConfig, 
        NotifierConfig notifierConfig,
        string? commitSha)
    {
        using var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", notifierConfig.Token);

        var message = new SlackMessageRequest
        {
            Channel = notifierConfig.Channel,
            Attachments = BuildMessage(image, repositoryConfig, commitSha)
        };
        
        HttpResponseMessage? response = null;
        try
        {
            response = await client.PostAsJsonAsync(Uri, message);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while sending slack notification");
            return new NotifierResult
            {
                StatusCode = response?.StatusCode ?? HttpStatusCode.InternalServerError,
                Error = e.Message,
                IsSuccess = false
            };
        }
        
        var stringResponse = await response.Content.ReadAsStringAsync();
        _logger.LogTrace("Slack response: {response}", stringResponse);
        
        var slackResponse = JsonSerializer.Deserialize<SlackMessageResponse>(stringResponse);
        if (!slackResponse.Ok)
        {
            _logger.LogWarning("Failed to send slack notification: {error}", slackResponse.Error);
            return new NotifierResult
            {
                IsSuccess = false,
                StatusCode = response.StatusCode,
                Error = slackResponse.Error
            };
        }
        
        _logger.LogInformation("Slack notification sent for image {image} in repository {repository}", 
            image.Name, 
            repositoryConfig.Name);

        return new NotifierResult
        {
            IsSuccess = true,
            StatusCode = response.StatusCode
        };
    }

    //Could be good to move over to a factory or builder pattern but feels overkill for now
    //Could be nice to also remove some of the magic strings but not overly concerned about this right now
    private List<Attachment> BuildMessage(Image image, RepositoryConfig repositoryConfig, string? commitSha)
    {
        var blocks = new List<Block>
        {
            new Block
            {
                Type = "section",
                Text = new Text
                {
                    Type = "mrkdwn",
                    Content = KurrentStrings.MessageHeader
                }
            },
            new Block
            {
                Type = "divider"
            },
            new Block
            {
                Type = "section",
                Text = new Text
                {
                    Type = "mrkdwn",
                    Content = KurrentStrings.MessageBody(image.Name, repositoryConfig.Url, repositoryConfig.Name)
                }
            },
        };

        if(repositoryConfig.Url.Contains("github.com"))
            blocks.Add(new Block
            {
                Type = "section",
                Text = new Text
                {
                    Type = "mrkdwn",
                    Content = KurrentStrings.CommitLink(commitSha, repositoryConfig.Url)
                }
            });
        
        blocks.Add(new Block
        {
            Type = "context",
            Elements = new List<Element>
            {
                new Element
                {
                    Type = "mrkdwn",
                    Text = KurrentStrings.MessageFooter(commitSha)
                }
            }
        });

        var attachment = new Attachment { Color = KurrentStrings.InformationBlue, Blocks = blocks};
        
        return new List<Attachment>{attachment};
    }
}