using FluentResults;
using RepositoryStats.GitHubApi;
using Serilog;

namespace RepositoryStats.Core.Steps;

public class DownloadStep
{
    private readonly IGitHubService _gitHubService;
        
    public DownloadStep(IGitHubService gitHubService)
    {
        _gitHubService = gitHubService;
    }
    
    public async Task<Result<byte[]>> Execute(string filePath)
    {
        Log.Debug("Downloading file {FilePath}", filePath);
        
        byte[] fileContent;
        try
        {
            fileContent = await _gitHubService.GetFileContent(filePath);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to download file content for {FilePath}", filePath);
            return Result.Fail<byte[]>("Failed to download file content");
        }

        return Result.Ok(fileContent);
    }
}