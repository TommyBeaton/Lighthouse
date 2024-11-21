using Kurrent.Models.Data.API;

namespace Kurrent.Interfaces.API;

public interface IAcrWrapper
{
    public Task<AcrListTagsResponse?> ListTags(string repoUrl, string image, string username, string password,
        CancellationToken ct);
    public Task<bool> ValidateConnection(string repoUrl, string username, string password, CancellationToken ct);
}