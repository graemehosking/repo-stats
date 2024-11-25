# RepositoryStats

A small exercise to count the frequency of letters in JavaScript/Typescript source in a GitHub repository.

Built as a console app in dotnet 8, using GitHub's Octokit nuget package to access GitHub repository resources.

## Configure

### GitHub API Access

The app will run with anonymous access to the GitHub API without further configuration. However, you may run into API rate limits as described here:

[Rate limits for the REST API](https://docs.github.com/en/rest/using-the-rest-api/rate-limits-for-the-rest-api?apiVersion=2022-11-28)

To get higher rate limits against the GitHub API you can use a GitHub personal access token (PAT). These can be created for an authenticated GitHub account [here](https://github.com/settings/tokens?type=beta).

Once you have a token it can be added as a user secret to the project. To do this, run the following command in the `./RepositoryStats.Cli/` directory:
```bash
dotnet user-secrets set "GitHubApiOptions:ApiKey" "<your_personal_access_token>"
```

### Repository Access

Configure the repository to be analysed in the GitHubApiOptions section of `appsettings.json`. The `Owner` and `Name` properties should be set to the owner and name of the repository respectively. The defaults will analyse the `lodash/lodash` repository.

```json
{
  "GitHubApiOptions": {
    "SearchRepository": {
      "Owner": "lodash",
      "Name": "lodash"
    }
  }
}
```

By default the app will create a pool of connections to query the chosen repository via the GitHub API. The size of this pool is configured in `appsettings.json` under `GitHubApiOptions:MaxConcurrentRequests`. The default value is 10. 

Setting a value of `1` will prevent any concurrent access to the GitHub repository. This will impact performance.

```json
{
  "GitHubApiOptions": {
    "SearchRepository": {
      "Owner": "lodash",
      "Name": "lodash"
    },
    "MaxConcurrentRequests": 10
  }
}
```
## Build
CD into the root of the repository and run the following command to build the project:
```bash
dotnet build
```

## Run

CD into the root of the repository and run the following command to build the project:
```bash
dotnet run 
```

## Logging

The app will output some reasonably verbose logging to the filesystem to help with troubleshooting. Look under `./logs/repositorystats*.log`.
