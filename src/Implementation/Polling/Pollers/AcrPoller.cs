using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Kurrent.Interfaces;
using Kurrent.Models.Data.Pollers;
using Kurrent.Utils;

namespace Kurrent.Implementation.Polling.Pollers;

public class AcrPoller : BasePoller
{
    private readonly ILogger<AcrPoller> _logger;

    protected override string Type => KurrentStrings.Acr;

    public AcrPoller(
        ISubscriptionHandler subscriptionHandler,
        IHttpClientFactory httpClientFactory,
        ILogger<AcrPoller> logger) : base(subscriptionHandler, httpClientFactory, logger)
    {
        _logger = logger;
    }
    
    protected override async Task<HttpResponseMessage?> MakeHttpRequest(HttpClient client, string image)
    {
        var authenticationString = $"{Config!.Username}:{Config!.Password}";
        var b64AuthString = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", b64AuthString);
        client.BaseAddress = new Uri($"https://{Config!.Url}/acr/v1/");
        return await client.GetAsync($"{image}/_tags?orderby=timedesc");
    }

    protected override string ExtractLatestTag(string jsonResponse)
    {
        var response = JsonSerializer.Deserialize<AcrResponse>(jsonResponse);
        if (response == null)
        {
            _logger.LogWarning("Failed to parse ACR response in poller: {pollerName}", Config);
            return string.Empty;
        }

        var sortedTags = response.Tags.OrderByDescending(tag => tag.CreatedTime).ToList();
        return sortedTags.First().Name;
    }
}