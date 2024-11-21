using System.Text.Json;
using Kurrent.Interfaces;
using Kurrent.Interfaces.API;
using Kurrent.Models.Data.API;
using Kurrent.Utils;
using Microsoft.Extensions.Options;

namespace Kurrent.Implementation.Polling.Pollers;

public class DockerHubPoller : BasePoller
{
    private readonly IDockerHubWrapper _dockerHubWrapper;
    private readonly ILogger<DockerHubPoller> _logger;

    protected override string Type => KurrentStrings.Docker;

    public DockerHubPoller(
        ISubscriptionHandler subscriptionHandler, 
        IDockerHubWrapper dockerHubWrapper,
        IHttpClientFactory httpClientFactory,
        ILogger<DockerHubPoller> logger) 
        : base(
            subscriptionHandler, 
            httpClientFactory, 
            logger)
    {
        _dockerHubWrapper = dockerHubWrapper;
        _logger = logger;
    }

    protected override async Task<string> GetLatestTag(string image, CancellationToken ct)
    {
        var response = await _dockerHubWrapper.ListTags(
            image, 
            Config.Username, 
            Config.Password, 
            ct);
        
        if (response == null)
        {
            _logger.LogWarning("Failed to get latest image from docker hub for image: {image}", image);
            return string.Empty;
        }

        var sortedTags = response.Results.OrderByDescending(tag => tag.TagLastPushed).ToList();
        return sortedTags.First().Name;
    }
}