namespace Kurrent.Interfaces.ExternalValidators;

public interface IExternalValidatorService
{
    public Task<bool> ValidateConfigAsync();
}