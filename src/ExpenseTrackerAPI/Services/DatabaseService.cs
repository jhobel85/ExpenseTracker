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

        var command = new SqlCommand("SELECT Id, Name, Description, CreatedAt FROM Categories ORDER BY Name", connection);
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

        var query = @"SELECT e.Id, e.Amount, e.CategoryId, e.Description, e.ExpenseDate, e.CreatedAt, c.Name as CategoryName
                      FROM Expenses e
                      INNER JOIN Categories c ON e.CategoryId = c.Id
                      WHERE (@StartDate IS NULL OR e.ExpenseDate >= @StartDate)
                        AND (@EndDate IS NULL OR e.ExpenseDate <= @EndDate)
                      ORDER BY e.ExpenseDate DESC";

        var command = new SqlCommand(query, connection);
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

        var query = @"SELECT e.Id, e.Amount, e.CategoryId, e.Description, e.ExpenseDate, e.CreatedAt, c.Name as CategoryName
                      FROM Expenses e
                      INNER JOIN Categories c ON e.CategoryId = c.Id
                      WHERE e.Id = @Id";

        var command = new SqlCommand(query, connection);
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

        var command = new SqlCommand(
            @"INSERT INTO Expenses (Amount, CategoryId, Description, ExpenseDate) 
              VALUES (@Amount, @CategoryId, @Description, @ExpenseDate);
              SELECT CAST(SCOPE_IDENTITY() as int);", connection);

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

        var command = new SqlCommand("DELETE FROM Expenses WHERE Id = @Id", connection);
        command.Parameters.AddWithValue("@Id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<Dictionary<string, decimal>> GetMonthlySummaryAsync(int year, int month)
    {
        var summary = new Dictionary<string, decimal>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"SELECT c.Name, ISNULL(SUM(e.Amount), 0) as Total
                      FROM Categories c
                      LEFT JOIN Expenses e ON c.Id = e.CategoryId 
                        AND YEAR(e.ExpenseDate) = @Year 
                        AND MONTH(e.ExpenseDate) = @Month
                      GROUP BY c.Name
                      ORDER BY Total DESC";

        var command = new SqlCommand(query, connection);
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
