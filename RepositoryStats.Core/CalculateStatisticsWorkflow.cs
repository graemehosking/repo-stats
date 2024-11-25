using RepositoryStats.Core.Steps;
using RepositoryStats.GitHubApi;
using Serilog;

namespace RepositoryStats.Core;

public class CalculateStatisticsWorkflow
{
    private readonly IGitHubService _gitHubService;
    private readonly DownloadStep _downloadStep;
    private readonly CalculateFileStatisticsStep _calculateFileStatisticsStep;
    private readonly UpdateAggregatedStatisticsStep _updateAggregatedStatisticsStep;
    
    public CalculateStatisticsWorkflow(
        IGitHubService gitHubService,
        DownloadStep downloadStep,
        CalculateFileStatisticsStep calculateFileStatisticsStep,
        UpdateAggregatedStatisticsStep updateAggregatedStatisticsStep)
    {
        _gitHubService = gitHubService;
        _downloadStep = downloadStep;
        _calculateFileStatisticsStep = calculateFileStatisticsStep;
        _updateAggregatedStatisticsStep = updateAggregatedStatisticsStep;
    }

    public async Task<IDictionary<char, int>> Run(CancellationToken token)
    {
        const string rootPath = "/";
        var processingSequences = new List<Task>();
        
        await foreach (var file in _gitHubService.GetFilesInPath(rootPath, token))
        {
            if (file.EndsWith(".js") || file.EndsWith(".ts"))
            {
                processingSequences.Add(RunProcessingSequenceForFile(file, token));
            }
        }

        await Task.WhenAll(processingSequences.ToArray());

        return _updateAggregatedStatisticsStep.GetAggregatedStatistics();
    }
    
    private async Task RunProcessingSequenceForFile(string filePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var fileDownloadResult = await _downloadStep.Execute(filePath);
        if (fileDownloadResult is { IsSuccess: false })
        {
            Log.Error("Failed to download file content for {filePath}", filePath);
            return;
        }
        
        cancellationToken.ThrowIfCancellationRequested();
        
        var fileStatsResult = await _calculateFileStatisticsStep.Execute(fileDownloadResult.Value);
        if (fileStatsResult is { IsSuccess: false })
        {
            Log.Error("Failed to calculate statistics for {filePath}", filePath);
            return;
        }
        
        var updateResult = await _updateAggregatedStatisticsStep.Execute(fileStatsResult.Value);
        if (updateResult is { IsSuccess: false })
        {
            Log.Error("Failed to update aggregated statistics for {filePath}", filePath);
            throw new ApplicationException("Failed to update aggregated statistics");
        }
    }
}