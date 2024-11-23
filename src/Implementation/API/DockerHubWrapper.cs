using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Kurrent.Interfaces.API;
using Kurrent.Models.Data.API;
using Kurrent.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Kurrent.Implementation.API;

public class DockerHubWrapper : IDockerHubWrapper
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly SystemConfig _systemConfig;
    private readonly ILogger<DockerHubWrapper> _logger;

    // If an image doesn't contain a '/' such as nginx then we can assume it's from the docker hub library
    // In which case we need to add library to the request
    private string GetImageTagsUrl(string image) =>
        $"{_systemConfig.DockerHubUri}/repositories/{(image.Contains('/') ? image : $"library/{image}")}/tags";
    
    private string GetTokenCacheKey(string username, string password) => $"DockerHubToken:{username}:{password}";
    
    public DockerHubWrapper(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache, 
        IOptions<SystemConfig> systemConfig,
        ILogger<DockerHubWrapper> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _systemConfig = systemConfig.Value;
        _logger = logger;
    }

    private async Task<DockerLoginResponse?> GetToken(string username, string password, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        var body = new DockerLoginRequest(username, password);
        var response = await client.PostAsJsonAsync($"{_systemConfig.DockerHubUri}/users/login", body, ct);
        try
        {
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var parsedResponse = JsonSerializer.Deserialize<DockerLoginResponse>(json, new JsonSerializerOptions{ PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower });
            if (parsedResponse == null)
            {
                _logger.LogWarning("Failed to parse response from DockerHub login for user: {username}", username);
                return null;
            }
            return parsedResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while getting token from DockerHub");
            return null;
        }
    }
    
    private async Task<string> GetTokenFromCache(string username, string password, CancellationToken ct)
    {
        if (!_cache.TryGetValue(GetTokenCacheKey(username, password), out DockerLoginResponse cachedToken) 
            || cachedToken.HasExpired)
        {
            var token = await GetToken(username, password, ct);
            if (token == null)
            {
                return string.Empty;
            }

            _cache.Set(GetTokenCacheKey(username, password), token);
        }

        return cachedToken.Token;
    }

    public async Task<DockerListTagsResponse?> ListTags(
        string image, 
        string username,
        string password,
        CancellationToken ct)
    {
        string token;
        var request = new HttpRequestMessage(HttpMethod.Get, GetImageTagsUrl(image));
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            token = await GetTokenFromCache(username, password, ct);
            if (token == string.Empty) return null;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        var client = _httpClientFactory.CreateClient();
        var response = await client.SendAsync(request, ct);
        try
        {
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var parsedResponse = JsonSerializer.Deserialize<DockerListTagsResponse>(json, new JsonSerializerOptions{ PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower });  //TODO: Check parsing options
            if (parsedResponse == null)
            {
                _logger.LogWarning("Failed to docker hub tags response for image: {image}", image);
                return null;
            }

            return parsedResponse;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception thrown while trying to get image tags from docker for image: {image}.", image);
            return null;
        }
    }

    public async Task<bool> ValidateConnection(string[] images, string username, string password, CancellationToken ct)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        foreach (var image in images)
        {
            var result = await ListTags(image, username, password, ct);
            if (result == null)
            {
                return false;
            }
        }
        return true;
    }
}