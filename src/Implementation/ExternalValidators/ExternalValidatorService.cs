using Kurrent.Interfaces.ExternalValidators;
using Kurrent.Utils;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.Configuration;

namespace Kurrent.Implementation.ExternalValidators;

// TODO: Move to a hosted service with a circuit breaker pattern
public class ExternalValidatorService : IExternalValidatorService
{
    private readonly AppConfig _appConfig;
    private readonly INotifierValidator _notifierValidator;
    private readonly IPollerValidator _pollerValidator;
    private readonly IRepositoryValidator _repositoryValidator;
    private readonly ILogger<ExternalValidatorService> _logger;

    public ExternalValidatorService(
        IOptions<AppConfig> appConfig,
        INotifierValidator notifierValidator, 
        IPollerValidator pollerValidator, 
        IRepositoryValidator repositoryValidator, 
        ILogger<ExternalValidatorService> logger)
    {
        _appConfig = appConfig.Value;
        _notifierValidator = notifierValidator;
        _pollerValidator = pollerValidator;
        _repositoryValidator = repositoryValidator;
        _logger = logger;
    }
    
    public async Task<(bool, List<string>?)> ValidateConfigAsync()
    {
        var validationTasks = new List<Task<(bool, List<string>?)>>();
        
        if (_appConfig.Repositories != null)
        {
            foreach (var repoConfig in _appConfig.Repositories)
            {
                validationTasks.Add(_repositoryValidator.Validate(repoConfig));
            }
        }
        
        if (_appConfig.Notifiers != null)
        {
            foreach (var notifierConfig in _appConfig.Notifiers)
            {
                validationTasks.Add(_notifierValidator.Validate(notifierConfig));
            }
        }
        
        if (_appConfig.Pollers != null)
        {
            foreach (var pollerConfig in _appConfig.Pollers)
            {
                validationTasks.Add(_pollerValidator.Validate(pollerConfig));
            }
        }

        var results = await Task.WhenAll(validationTasks);

        // If any external services failed to validate, return collection of errors.
        if (results.Any(result => !result.Item1))
        {
            _logger.LogError("Some external services failed to validate. Check logs for more detials.");
            return (false, results.Where(x => x.Item2 != null).SelectMany(x => x.Item2).ToList());
        }

        _logger.LogInformation("All external services validated successfully.");
        return (true, null);
    }
}