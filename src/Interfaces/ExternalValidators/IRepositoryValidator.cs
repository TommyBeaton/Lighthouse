using Kurrent.Utils;

namespace Kurrent.Interfaces.ExternalValidators;

public interface IRepositoryValidator
{
    public Task<bool> IsValid(RepositoryConfig config);
}