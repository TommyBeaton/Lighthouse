using Kurrent.Interfaces.Git;
using Kurrent.Utils;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace Kurrent.Implementation.Git;

public class GitService : IGitService
{
    private readonly ILogger<GitService> _logger;

    public GitService(ILogger<GitService> logger)
    {
        _logger = logger;
    }
    
    public Repository? CloneAndCheckout(RepositoryConfig repositoryConfig)
    {
        var cloneOptions = new CloneOptions
        {
            CredentialsProvider = GetCredentialsHandler(repositoryConfig)
        };

        try
        {
            var localPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var repoPath = Repository.Clone(repositoryConfig.Url, localPath, cloneOptions);
            var repo = new Repository(repoPath);
            Commands.Checkout(repo, repo.Branches[repositoryConfig.Branch]);
            return repo;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error cloning repository");
            return null;
        }
    }

    public bool CheckoutBranch(Repository repo, string branchName)
    {
        Branch branch;
        
        try
        {
            branch = repo.Branches[branchName];
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error finding branch: {branchName}", branchName);
            return false;
        }
        
        if (branch == null)
        {
            _logger.LogError("Branch: {branchName} not found.", branchName);
            return false;
        }
        
        Commands.Checkout(repo, branch);
        return true;
    }

    public string CommitAndPushChanges(Repository clonedRepo,
        RepositoryConfig repositoryConfig,
        string commitMessage = "")
    {
        _logger.LogTrace("Committing changes to repository: {repositoryName}", repositoryConfig.Name);
        
        var signature = new Signature("Kurrent Service", "service@kurrent.tech", DateTimeOffset.Now);
        var commit = clonedRepo.Commit(commitMessage, signature, signature);
        var pushOptions = new PushOptions
        {
            CredentialsProvider = GetCredentialsHandler(repositoryConfig)
        };
        
        _logger.LogInformation("Pushing changes to repository: {repository}", repositoryConfig.Name);
        clonedRepo.Network.Push(clonedRepo.Branches[repositoryConfig.Branch], pushOptions);
        return commit.Sha;
    }

    public bool HasChanges(Repository repo)
    {
        return repo.RetrieveStatus().IsDirty;
    }
    
    private  CredentialsHandler GetCredentialsHandler(RepositoryConfig repositoryConfig)
    {
        return (_, _, _) => new UsernamePasswordCredentials
        {
            Username = repositoryConfig.Username,
            Password = repositoryConfig.Password
        };
    }
}