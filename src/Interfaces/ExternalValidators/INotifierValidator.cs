using Kurrent.Utils;

namespace Kurrent.Interfaces.ExternalValidators;

public interface INotifierValidator
{
    public Task<bool> IsValid(NotifierConfig config);
}