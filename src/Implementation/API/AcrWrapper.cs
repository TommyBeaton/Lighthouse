using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Kurrent.Interfaces.API;
using Kurrent.Models.Data.API;

namespace Kurrent.Implementation.API;

//TODO: Move repository wrappers to common interface and factory pattern.
public class AcrWrapper: IAcrWrapper
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AcrWrapper> _logger;

    public AcrWrapper(
        IHttpClientFactory httpClientFactory,
        ILogger<AcrWrapper> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private HttpClient GetAuthenticatedHttpClient(string username, string password)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        return httpClient;
    }
    
    public async Task<AcrListTagsResponse?> ListTags(string repoUrl, string image, string username, string password, CancellationToken ct)
    {
        var httpClient = GetAuthenticatedHttpClient(username, password);
        httpClient.BaseAddress = new Uri($"https://{repoUrl}/acr/v1/");
        var response =  await httpClient.GetAsync($"{image}/_tags?orderby=timedesc");
        try
        {
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var parsedResponse = JsonSerializer.Deserialize<AcrListTagsResponse>(json);  //TODO: Check parsing options
            if (parsedResponse == null)
            {
                _logger.LogWarning("Failed to parse image tags response from acr in repo: {repoUrl}", repoUrl);
                return null;
            }

            return parsedResponse;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception thrown while trying to get image tags from: {repoUrl}.", repoUrl);
            return null;
        }
    }

    public async Task<bool> ValidateConnection(string repoUrl, string username, string password, CancellationToken ct)
    {
        var httpClient = GetAuthenticatedHttpClient(username, password);
        var url = $"https://{repoUrl}/v2/_catalog";
        var response = await httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation($"Successfully connected to ACR poller at '{repoUrl}'.");
        return true;
    }
}