namespace RepositoryStats.GitHubApi.Configuration;

public sealed class SearchRepositoryOptions
{
    public required string Owner { get; set; }
    public required string Name { get; set; }
}