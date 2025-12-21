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

        // Read and execute schema.sql
        var schemaPath = Path.GetFullPath(Path.Combine(contentRootPath, "..", "..", "Database", "schema.sql"));
        if (File.Exists(schemaPath))
        {
            var schemaSql = await File.ReadAllTextAsync(schemaPath);
            var parts = schemaSql.Split("CREATE OR ALTER PROCEDURE", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                // Split DDL part by CREATE VIEW
                var ddlParts = parts[0].Split("CREATE VIEW", StringSplitOptions.RemoveEmptyEntries);
                if (ddlParts.Length > 0)
                {
                    // Execute tables and inserts, remove comments
                    var tiLines = ddlParts[0].Split('\n').Where(line => !line.Trim().StartsWith("--"));
                    var tiSql = string.Join('\n', tiLines);
                    var tiCommand = new SqlCommand(tiSql, connection);
                    await tiCommand.ExecuteNonQueryAsync();
                    
                    // Execute view, remove comments
                    var viewLines = ("CREATE VIEW" + ddlParts[1]).Split('\n').Where(line => !line.Trim().StartsWith("--"));
                    var viewSql = string.Join('\n', viewLines);
                    var viewCommand = new SqlCommand(viewSql, connection);
                    await viewCommand.ExecuteNonQueryAsync();
                }
                
                // Execute each procedure separately, remove comments
                for (int i = 1; i < parts.Length; i++)
                {
                    var procLines = ("CREATE OR ALTER PROCEDURE" + parts[i]).Split('\n').Where(line => !line.Trim().StartsWith("--"));
                    var procSql = string.Join('\n', procLines);
                    var procCommand = new SqlCommand(procSql, connection);
                    await procCommand.ExecuteNonQueryAsync();
                }
            }
        }
        else
        {
            throw new FileNotFoundException("schema.sql not found", schemaPath);
        }
    }
}