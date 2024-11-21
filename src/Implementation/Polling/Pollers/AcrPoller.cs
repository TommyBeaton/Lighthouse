using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Kurrent.Interfaces;
using Kurrent.Interfaces.API;
using Kurrent.Models.Data.API;
using Kurrent.Utils;

namespace Kurrent.Implementation.Polling.Pollers;

public class AcrPoller : BasePoller
{
    private readonly IAcrWrapper _acrWrapper;
    private readonly ILogger<AcrPoller> _logger;

    protected override string Type => KurrentStrings.Acr;

    public AcrPoller(
        IAcrWrapper acrWrapper,
        ISubscriptionHandler subscriptionHandler,
        IHttpClientFactory httpClientFactory,
        ILogger<AcrPoller> logger) : base(subscriptionHandler, httpClientFactory, logger)
    {
        _acrWrapper = acrWrapper;
        _logger = logger;
    }

    protected override async Task<string> GetLatestTag(string image, CancellationToken ct)
    {
        var response = await _acrWrapper.ListTags(
            Config.Url,
            image, 
            Config.Username, 
            Config.Password, 
            ct);

        if (response == null)
        {
            _logger.LogWarning("Failed to get latest tag from ACR in for image: {image}", image);
            return string.Empty;
        }

        var sortedTags = response.Tags.OrderByDescending(tag => tag.CreatedTime).ToList();
        return sortedTags.First().Name;
    }
}