namespace Kurrent.Interfaces.ExternalValidators;

public interface IExternalValidator<T>
{
    public Task<(bool, List<string>?)> Validate(T config);
}