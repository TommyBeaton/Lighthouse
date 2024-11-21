using Kurrent.Utils;

namespace Kurrent.Interfaces.ExternalValidators;

public interface IPollerValidator
{
    public Task<bool> IsValid(PollerConfig config);
}