using System.Text;
using RepositoryStats.Core;

namespace RepositoryStats.Cli;

public sealed class RepositoryStatsApp
{
    private readonly CalculateStatisticsWorkflow _workflow;
    
    public RepositoryStatsApp(CalculateStatisticsWorkflow workflow)
    {
        _workflow = workflow;
    }
    
    public async Task Run()
    {
        Console.WriteLine("Gathering repository statistics. Press 'C' to cancel...");
        
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        try
        {
            var repositoryStatsTask = _workflow.Run(cancellationToken);
        
            while (!cancellationToken.IsCancellationRequested && !repositoryStatsTask.IsCompleted)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.C)
                {
                    Console.WriteLine("Cancelling...");
                    Log.Information("Cancellation requested");
                    await cts.CancelAsync();
                }

                await Task.Delay(20, cancellationToken); // cool that tight loop a bit
            }
        
            var formattedStats = BuildFormattedStatsLines(repositoryStatsTask.Result);
        
            Console.Write(formattedStats);
            Log.Information(formattedStats);
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("Operation was cancelled");
        }
    }
    
    private string BuildFormattedStatsLines(IDictionary<char, int> repositoryStats)
    {
        var logOutput = new StringBuilder();

        logOutput.AppendLine($"Found a total of {repositoryStats.Keys.Count} unique letters" + Environment.NewLine);
        logOutput.AppendLine("---------------------");
        logOutput.AppendLine("| Letter | Count    |");
        logOutput.AppendLine("---------------------");

        foreach (var (letter, count) in repositoryStats.OrderByDescending(x => x.Value))
        {
            logOutput.AppendLine($"| {letter.ToString(),-6} | {count.ToString(),-8} |");
        }

        logOutput.AppendLine("---------------------");

        return logOutput.ToString();
    }
}