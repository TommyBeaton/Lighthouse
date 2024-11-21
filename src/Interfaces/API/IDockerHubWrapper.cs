using Kurrent.Models.Data.API;

namespace Kurrent.Interfaces.API;

public interface IDockerHubWrapper
{
    public Task<DockerListTagsResponse?> ListTags(
        string image, 
        string username, 
        string password,
        CancellationToken ct);
    
    public Task<bool> ValidateConnection(
        string[] images, 
        string username,
        string password,
        CancellationToken ct);
}