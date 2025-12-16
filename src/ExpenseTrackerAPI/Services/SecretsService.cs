using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Text.Json;

namespace ExpenseTrackerAPI.Services;

public class SecretsService
{
    private readonly IAmazonSecretsManager _secretsManager;

    public SecretsService()
    {
        _secretsManager = new AmazonSecretsManagerClient();
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        var request = new GetSecretValueRequest
        {
            SecretId = secretName
        };

        var response = await _secretsManager.GetSecretValueAsync(request);
        return response.SecretString;
    }

    public async Task<DatabaseCredentials> GetDatabaseCredentialsAsync(string secretName)
    {
        var secretValue = await GetSecretAsync(secretName);
        return JsonSerializer.Deserialize<DatabaseCredentials>(secretValue)!;
    }
}

public class DatabaseCredentials
{
    public string username { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
    public string engine { get; set; } = string.Empty;
    public string host { get; set; } = string.Empty;
    public int port { get; set; }
    public string dbname { get; set; } = string.Empty;
}