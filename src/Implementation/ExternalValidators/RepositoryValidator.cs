using Kurrent.Interfaces.ExternalValidators;
using Kurrent.Interfaces.Git;
using Kurrent.Utils;

namespace Kurrent.Implementation.ExternalValidators;

public class RepositoryValidator : IRepositoryValidator
{
    private readonly IGitService _gitService;
    private readonly ILogger _logger;

    public RepositoryValidator(
        IGitService gitService,
        ILogger logger)
    {
        _gitService = gitService;
        _logger = logger;
    }
    
    public async Task<bool> IsValid(RepositoryConfig config)
    {
        var repo = _gitService.CloneAndCheckout(config);
        if (repo is null)
        {
            _logger.LogError($"Failed to connect to repository '{config.Name}'");
            return false;
        }
        
        _logger.LogInformation($"Successfully connected to repository '{config.Name}'.");
        return true;
    }
}