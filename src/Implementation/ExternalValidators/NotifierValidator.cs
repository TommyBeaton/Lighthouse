using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kurrent.Interfaces.ExternalValidators;
using Kurrent.Models.Data.Notifiers;
using Kurrent.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Kurrent.Implementation.ExternalValidators;

public class NotifierValidator : INotifierValidator
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NotifierValidator> _logger;

    public NotifierValidator(
        IHttpClientFactory httpClientFactory,
        ILogger<NotifierValidator> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    
    public async Task<(bool, List<string>?)> Validate(NotifierConfig config)
    {
        try
        {
            if (config.Type == "slack")
            {
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.Token);
                var response = await httpClient.PostAsync("https://slack.com/api/auth.test", null);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<SlackAuthResponse>(content, new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
                if (jsonResponse == null)
                {
                    throw new JsonException("Deserialization of Slack Auth response failed.");
                }
                bool success = jsonResponse.Ok;
                List<string>? errors = null;
                if (success)
                {
                    _logger.LogInformation($"Successfully connected to Slack notifier '{config.Name}'.");
                }
                else
                {
                    _logger.LogError($"Failed to authenticate with Slack notifier '{config.Name}'. Response: {content}");
                    errors =
                    [
                        "Failed to authenticate with Slack notifier '" + config.Name + "'."
                    ];
                }

                return (success, errors);
            }

            _logger.LogWarning($"Notifier type '{config.Type}' is not supported.");
            return (false, [$"Notifier type '{config.Type}' is not supported."]);
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to connect to notifier '{config.Name}': {ex.Message}");
            return (false, [$"Failed to connect to notifier '{config.Name}'"]);
        }
    }
}