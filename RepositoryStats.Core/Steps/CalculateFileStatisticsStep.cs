using System.Text;
using FluentResults;
using Serilog;

namespace RepositoryStats.Core.Steps;

public class CalculateFileStatisticsStep
{
    public async Task<Result<Dictionary<char, int>>> Execute(byte[] fileContent)
    {
        ArgumentNullException.ThrowIfNull(fileContent, nameof(fileContent));
        
        Log.Debug("Calculating statistics over {Length} bytes", fileContent.Length);
        
        var result = new Dictionary<char, int>();
        
        if (fileContent.Length == 0)
        {
            // consider this a success case
            return Result.Ok(result);
        }

        try
        {
            // assumption: sources files will be Utf8, or compatible with it
            // double-byte character encoding may not work so well...
            var utf8Content = Encoding.UTF8.GetString(fileContent);

            foreach (char c in utf8Content)
            {
                // assumption: we strictly count letters only as this is in the
                // specification (letter not character), and we respect case sensitivity (A != a)
                if (char.IsLetter(c))
                {
                    if (!result.TryAdd(c, 1))
                    {
                        result[c]++;
                    }
                }
            }
        }
        catch (ArgumentException e)
        {
            Log.Error("Invalid Unicode characters encountered: {Message}", e.Message);
            return Result.Fail(e.Message);
        }
        catch (Exception e)
        {
            Log.Error("Error encountered in {MethodName}: {Message}", nameof(Execute), e.Message);
            return Result.Fail(e.Message);
        }

        return await Task.FromResult(Result.Ok(result));
    }
}