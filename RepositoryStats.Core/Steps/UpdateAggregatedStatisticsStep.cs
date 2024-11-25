using System.Collections.Concurrent;
using FluentResults;
using Serilog;

namespace RepositoryStats.Core.Steps;

public class UpdateAggregatedStatisticsStep
{
    private readonly Dictionary<char, int> _aggregatedStatistics = new ();
    private readonly SemaphoreSlim _aggregatedStatisticsLock = new(1, 1);
    
    // public UpdateAggregatedStatisticsStep()
    // {
    // }
    
    public IDictionary<char, int> GetAggregatedStatistics()
    {
        try
        {
            _aggregatedStatisticsLock.Wait();
            
            return new Dictionary<char, int>(_aggregatedStatistics);
        }
        finally
        {
            _aggregatedStatisticsLock.Release();
        }
    }
    
    public async Task<Result> Execute(IDictionary<char, int> newFileStatistics)
    {
        Log.Debug("Updating aggregated statistics with {Keys} new values", 
            newFileStatistics.Keys.Count);
        
        await _aggregatedStatisticsLock.WaitAsync();
        
        try
        {
            foreach (var (key, value) in newFileStatistics)
            {
                if (!_aggregatedStatistics.TryAdd(key, value))
                {
                    _aggregatedStatistics[key] += value;
                }
            }
        }
        catch (Exception e)
        {
            Log.Error("Error encountered in {MethodName}: {Message}", nameof(Execute), e.Message);
            return Result.Fail(e.Message);
        }
        finally
        {
            _aggregatedStatisticsLock.Release();
        }
        
        return Result.Ok();
    }
}