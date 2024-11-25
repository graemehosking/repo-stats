namespace RepositoryStats.GitHubApi.Configuration;

public class GitHubApiOptions
{
    public GitHubApiOptions() { }
    
    public required string ApiKey { get; set; }
    public required int MaxConcurrentRequests { get; set; }
    public required SearchRepositoryOptions SearchRepository { get; set; }
}