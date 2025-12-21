using Microsoft.Data.SqlClient;
using ExpenseTrackerAPI.Models;

namespace ExpenseTrackerAPI.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
    }

    // Categories
    public async Task<List<Category>> GetCategoriesAsync()
    {
        var categories = new List<Category>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("EXEC GetCategories", connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            categories.Add(new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                CreatedAt = reader.GetDateTime(3)
            });
        }

        return categories;
    }

    // Expenses
    public async Task<List<Expense>> GetExpensesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var expenses = new List<Expense>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("EXEC GetExpenses @StartDate, @EndDate", connection);
        command.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@EndDate", (object?)endDate ?? DBNull.Value);

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            expenses.Add(new Expense
            {
                Id = reader.GetInt32(0),
                Amount = reader.GetDecimal(1),
                CategoryId = reader.GetInt32(2),
                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                ExpenseDate = reader.GetDateTime(4),
                CreatedAt = reader.GetDateTime(5),
                CategoryName = reader.GetString(6)
            });
        }

        return expenses;
    }

    public async Task<Expense?> GetExpenseByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("EXEC GetExpenseById @Id", connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Expense
            {
                Id = reader.GetInt32(0),
                Amount = reader.GetDecimal(1),
                CategoryId = reader.GetInt32(2),
                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                ExpenseDate = reader.GetDateTime(4),
                CreatedAt = reader.GetDateTime(5),
                CategoryName = reader.GetString(6)
            };
        }

        return null;
    }

    public async Task<int> CreateExpenseAsync(CreateExpenseRequest request)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("EXEC CreateExpense @Amount, @CategoryId, @Description, @ExpenseDate", connection);

        command.Parameters.AddWithValue("@Amount", request.Amount);
        command.Parameters.AddWithValue("@CategoryId", request.CategoryId);
        command.Parameters.AddWithValue("@Description", (object?)request.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@ExpenseDate", request.ExpenseDate);

        var id = await command.ExecuteScalarAsync();
        return (int)id!;
    }

    public async Task<bool> DeleteExpenseAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("EXEC DeleteExpense @Id", connection);
        command.Parameters.AddWithValue("@Id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<Dictionary<string, decimal>> GetMonthlySummaryAsync(int year, int month)
    {
        var summary = new Dictionary<string, decimal>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("EXEC GetMonthlySummary @Year, @Month", connection);
        command.Parameters.AddWithValue("@Year", year);
        command.Parameters.AddWithValue("@Month", month);

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            summary[reader.GetString(0)] = reader.GetDecimal(1);
        }

        return summary;
    }
}
