using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RepositoryStats.Cli;
using RepositoryStats.Core;
using RepositoryStats.Core.Steps;
using RepositoryStats.GitHubApi;
using RepositoryStats.GitHubApi.Configuration;

var appConfiguration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(appConfiguration)
    .CreateLogger();

try
{
    Log.Information("Startup");
    
    var serviceProvider = ConfigureServiceProvider(appConfiguration);
    var app = serviceProvider.GetRequiredService<RepositoryStatsApp>();
    
    await app.Run();
}
catch (Exception ex)
{
    Log.Error(ex, "Something went very wrong");
}
finally
{
    Log.Information("Shutdown");
}

ServiceProvider ConfigureServiceProvider(IConfigurationRoot configurationRoot)
{
    var serviceCollection = new ServiceCollection();
    ConfigureDiWithConfiguration(serviceCollection, configurationRoot);
    ConfigureDiForServices(serviceCollection);
    
    return serviceCollection.BuildServiceProvider();

    void ConfigureDiWithConfiguration(ServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<GitHubApiOptions>().Bind(configuration.GetSection(nameof(GitHubApiOptions)));
    }

    void ConfigureDiForServices(IServiceCollection services)
    {
        services.AddSingleton<IGitHubService, GitHubService>();
        services.AddSingleton<DownloadStep>();
        services.AddSingleton<CalculateFileStatisticsStep>();
        services.AddSingleton<UpdateAggregatedStatisticsStep>();
        services.AddSingleton<CalculateStatisticsWorkflow>();
        services.AddSingleton<RepositoryStatsApp>();
    }
}