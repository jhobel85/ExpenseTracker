using Microsoft.Data.SqlClient;
using System.IO;

namespace ExpenseTrackerAPI;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(string connectionString, string contentRootPath)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Create database if it doesn't exist
        var createDbCommand = new SqlCommand(@"
            IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ExpenseTracker')
            BEGIN
                CREATE DATABASE ExpenseTracker
            END", connection);
        
        await createDbCommand.ExecuteNonQueryAsync();
        
        // Switch to ExpenseTracker database
        var useDbCommand = new SqlCommand("USE ExpenseTracker", connection);
        await useDbCommand.ExecuteNonQueryAsync();

        // Read and execute schema.sql batches (split by GO)
        var schemaPath = Path.GetFullPath(Path.Combine(contentRootPath, "..", "..", "Database", "schema.sql"));
        if (File.Exists(schemaPath))
        {
            var schemaSql = await File.ReadAllTextAsync(schemaPath);
            var batches = schemaSql.Split("GO", StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var batch in batches)
            {
                var batchSql = batch.Trim();
                if (!string.IsNullOrWhiteSpace(batchSql))
                {
                    var command = new SqlCommand(batchSql, connection);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        else
        {
            throw new FileNotFoundException("schema.sql not found", schemaPath);
        }
    }
}