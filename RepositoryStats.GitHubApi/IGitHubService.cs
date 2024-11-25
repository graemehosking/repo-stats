namespace RepositoryStats.GitHubApi;

public interface IGitHubService
{
    IAsyncEnumerable<string> GetFilesInPath(string path, CancellationToken token);
    Task<byte[]> GetFileContent(string filePath);
}