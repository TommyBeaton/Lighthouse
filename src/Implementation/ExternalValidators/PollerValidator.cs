using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Kurrent.Interfaces.ExternalValidators;
using Kurrent.Models.Data.Pollers;
using Kurrent.Utils;
using Microsoft.Extensions.Options;

namespace Kurrent.Implementation.ExternalValidators;

public class PollerValidator : IPollerValidator
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SystemConfig _systemConfig;
    private readonly ILogger<PollerValidator> _logger;

    public PollerValidator(
        IHttpClientFactory httpClientFactory, 
        IOptions<SystemConfig> systemConfig,
        ILogger<PollerValidator> logger)
    {
        _httpClientFactory = httpClientFactory;
        _systemConfig = systemConfig.Value;
        _logger = logger;
    }
    
    public async Task<bool> IsValid(PollerConfig config)
    {
        try
        {
            switch (config.Type)
            {
                case KurrentStrings.Acr:
                    return await IsAcrConnectionValid(config);
                case KurrentStrings.Docker:
                    return await IsDockerHubConnectionValid(config);
                default:
                    _logger.LogWarning($"Poller type '{config.Type}' is not supported.");
                    return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to connect to poller '{config.EventName}': {ex.Message}");
            return false;
        }
    }

    private async Task<bool> IsAcrConnectionValid(PollerConfig config)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        var byteArray = Encoding.ASCII.GetBytes($"{config.Username}:{config.Password}");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        var url = $"{config.Url}/v2/_catalog";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation($"Successfully connected to ACR poller '{config.EventName}'.");
        return true;
    }

    private async Task<bool> IsDockerHubConnectionValid(PollerConfig config)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_systemConfig.DockerHubUri}/v2/_dockerhub");
        if (!string.IsNullOrEmpty(config.Username) && !string.IsNullOrEmpty(config.Password))
        {
            
            var body = new DockerLoginRequest(config.Username, config.Password);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            
        }
        
        // Docker Hub API requires OAuth authentication; for this example, we'll attempt a simple GET
        var response = await httpClient.GetAsync("https://registry-1.docker.io/v2/");
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Handle authentication if necessary
            _logger.LogError($"Authentication required for Docker poller '{config.EventName}'.");
            return false;
        }

        response.EnsureSuccessStatusCode();
        _logger.LogInformation($"Successfully connected to Docker poller '{config.EventName}'.");
        return true;
    }
}