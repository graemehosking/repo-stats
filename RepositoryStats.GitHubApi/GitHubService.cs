using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using Octokit;
using RepositoryStats.GitHubApi.Configuration;
using Serilog;

namespace RepositoryStats.GitHubApi;

public sealed class GitHubService : IGitHubService
{
    private const int MaxRetryAttempts = 3;
    private const int RetryDelayMilliseconds = 1000;
    private readonly int _maxConcurrentRequests;
    
    private readonly GitHubApiOptions _apiOptions;
    private readonly SemaphoreSlim _clientSemaphore = new(1, 1);
    private readonly ConcurrentBag<GitHubClient> _clientPool = new();
    private int _clientsCreated;
    
    public GitHubService(IOptions<GitHubApiOptions> options)
    {
        _apiOptions = options.Value;
        
        _maxConcurrentRequests = _apiOptions.MaxConcurrentRequests;
        if (_maxConcurrentRequests < 1)
        {
            _maxConcurrentRequests = 10;
            Log.Warning("MaxConcurrentRequests is less than 1, defaulting to 10");
        }
    }
    
    public async IAsyncEnumerable<string> GetFilesInPath(string path, [EnumeratorCancellation] CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        
        var client = await GetClient();
        try
        {
            await foreach (var file in InternalGetFilesInPath(client, path).WithCancellation(token))
            {
                yield return file;
            }
        }
        finally
        {
            ReturnClient(client);
        }
    }

    public async Task<byte[]> GetFileContent(string filePath)
    {
        Log.Verbose("GetFileContent: {FilePath}", filePath);
        
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        var client = await GetClient();
        try
        {
            return await ExecuteWithRetryAsync(async () => 
                await client.Repository.Content.GetRawContent(
                    _apiOptions.SearchRepository.Owner,
                    _apiOptions.SearchRepository.Name,
                    filePath));
        }
        finally
        {
            ReturnClient(client);
        }
    }
    
    private async IAsyncEnumerable<string> InternalGetFilesInPath(GitHubClient client, string path)
    {
        Log.Verbose("InternalGetFilesInPath: {Path}", path);
        
        var pathContents = await ExecuteWithRetryAsync(async () =>
            await client.Repository.Content.GetAllContents(
                _apiOptions.SearchRepository.Owner,
                _apiOptions.SearchRepository.Name,
                path));

        foreach (var contentItem in pathContents)
        {
            if (contentItem.Type == ContentType.Dir)
            {
                // recursively get files in the subdirectory, yield each file as its discovered
                await foreach (var discoveredFile in InternalGetFilesInPath(client, contentItem.Path))
                {
                    yield return discoveredFile;
                }
            }
            else if (contentItem.Type == ContentType.File)
            {
                Log.Verbose("File found: {Path}", contentItem.Path);

                yield return contentItem.Path;
            }
        }
    }
    
    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> apiAction)
    {
        int retryAttempts = 0;
        while (true)
        {
            try
            {
                return await apiAction();
            }
            catch
            {
                retryAttempts++;
                if (retryAttempts >= MaxRetryAttempts)
                {
                    throw;
                }
                
                var waitFor = RetryDelayMilliseconds * retryAttempts;
                Log.Warning("API error - retry in {RetryDelayMilliseconds}ms", waitFor);
                
                await Task.Delay(waitFor);
            }
        }
    }
    
    /// <summary>
    /// Safely get a client from the pool or create a new one
    /// </summary>
    private async Task<GitHubClient> GetClient()
    {
        await _clientSemaphore.WaitAsync();

        GitHubClient? poolClient;
        try
        {
            if (_clientPool.IsEmpty && _clientsCreated < _maxConcurrentRequests)
            {
                _clientPool.Add(CreateClient());
                _clientsCreated++;
            }
            
            while (!_clientPool.TryTake(out poolClient))
            {
                await Task.Delay(10);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get client");
            throw;
        }
        finally
        {
            _clientSemaphore.Release();
        }

        if (poolClient is null)
        {
            throw new InvalidOperationException("Failed to get a client (NULL)");
        }
        
        return poolClient;
    }

    private void ReturnClient(GitHubClient client)
    {
        // put it\\the client back in our pool
        _clientPool.Add(client);
    }
        
    private GitHubClient CreateClient()
    {
        const string clientName = "LodashStats.GitHubApi";
        
        if (string.IsNullOrEmpty(_apiOptions.ApiKey))
        {
            Log.Warning("Creating client with anonymous API access");
            return new GitHubClient(new ProductHeaderValue(clientName));
        }
        
        Log.Information("Creating client with API key {ApiKey}", _apiOptions.ApiKey.Substring(0, 20) + "...");
        return new GitHubClient(new ProductHeaderValue(clientName))
        {
            Credentials = new Credentials(_apiOptions.ApiKey) 
        };
    }
}