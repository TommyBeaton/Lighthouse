using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Kurrent.Interfaces.API;
using Kurrent.Interfaces.ExternalValidators;
using Kurrent.Models.Data.API;
using Kurrent.Utils;
using Microsoft.Extensions.Options;

namespace Kurrent.Implementation.ExternalValidators;

public class PollerValidator : IPollerValidator
{
    private readonly IAcrWrapper _acrWrapper;
    private readonly IDockerHubWrapper _dockerHubWrapper;
    private readonly SystemConfig _systemConfig;
    private readonly ILogger<PollerValidator> _logger;

    public PollerValidator(
        IAcrWrapper acrWrapper,
        IDockerHubWrapper dockerHubWrapper,
        IOptions<SystemConfig> systemConfig,
        ILogger<PollerValidator> logger)
    {
        _acrWrapper = acrWrapper;
        _dockerHubWrapper = dockerHubWrapper;
        _systemConfig = systemConfig.Value;
        _logger = logger;
    }
    
    public async Task<(bool, List<string>?)> Validate(PollerConfig config)
    {
        bool success = false;
        try
        {
            switch (config.Type)
            {
                case KurrentStrings.Acr:
                    success = await _acrWrapper.ValidateConnection(config.Url, config.Username, config.Password, CancellationToken.None);
                    break;
                case KurrentStrings.Docker:
                    success = await _dockerHubWrapper.ValidateConnection(config.Images, config.Username, config.Password, CancellationToken.None);
                    break;
                default:
                    _logger.LogWarning($"Poller type '{config.Type}' is not supported.");
                    return (false, [$"Poller type '{config.Type}' is not supported."]);
            }
            List<string>? errors = success ? null : ["Poller validation failed."];
            return (success, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to connect to poller '{config.EventName}': {ex.Message}");
            return (false, [$"Failed to connect to poller '{config.EventName}'"]);
        }
    }
}