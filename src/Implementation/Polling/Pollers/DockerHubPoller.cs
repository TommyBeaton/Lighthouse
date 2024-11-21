using System.Text.Json;
using Kurrent.Interfaces;
using Kurrent.Models.Data.Pollers;
using Kurrent.Utils;
using Microsoft.Extensions.Options;

namespace Kurrent.Implementation.Polling.Pollers;

public class DockerHubPoller : BasePoller
{
    private readonly SystemConfig _systemConfig;
    private readonly ILogger<DockerHubPoller> _logger;

    protected override string Type => KurrentStrings.Docker;
    // If an image doesn't contain a '/' such as nginx then we can assume it's from the docker hub library
    // In which case we need to add library to the request
    private string GetImageTagsUrl(string image) =>
        $"{_systemConfig.DockerHubUri}/repositories/{(image.Contains('/') ? image : $"library/{image}")}/tags";

    public DockerHubPoller(
        ISubscriptionHandler subscriptionHandler, 
        IHttpClientFactory httpClientFactory,
        IOptions<SystemConfig> systemConfig,
        ILogger<DockerHubPoller> logger) 
        : base(
            subscriptionHandler, 
            httpClientFactory, 
            logger)
    {
        _systemConfig = systemConfig.Value;
        _logger = logger;
    }
    
    protected override async Task<HttpResponseMessage?> MakeHttpRequest(HttpClient client, string image)
    {
        return await client.GetAsync(GetImageTagsUrl(image));
    }

    protected override string ExtractLatestTag(string jsonResponse)
    {
        var response = JsonSerializer.Deserialize<DockerResponse>(jsonResponse);
        if (response == null)
        {
            _logger.LogWarning("Failed to parse response in poller: {pollerName}", Config);
            return string.Empty;
        }

        var sortedTags = response.Results.OrderByDescending(tag => tag.TagLastPushed).ToList();
        return sortedTags.First().Name;
    }
}