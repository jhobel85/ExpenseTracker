using Microsoft.Data.SqlClient;

namespace ExpenseTrackerAPI;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(string connectionString)
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

        // Create Categories table
        var createCategoriesCommand = new SqlCommand(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Categories' AND xtype='U')
            BEGIN
                CREATE TABLE Categories (
                    Id int IDENTITY(1,1) PRIMARY KEY,
                    Name nvarchar(100) NOT NULL,
                    Description nvarchar(500),
                    CreatedAt datetime2 DEFAULT GETDATE()
                )
                
                INSERT INTO Categories (Name, Description) VALUES
                ('Food', 'Meals, groceries, dining out'),
                ('Transport', 'Gas, public transport, parking'),
                ('Bills', 'Utilities, phone, internet'),
                ('Entertainment', 'Movies, games, hobbies'),
                ('Shopping', 'Clothes, electronics, misc items'),
                ('Health', 'Medical, pharmacy, fitness'),
                ('Other', 'Miscellaneous expenses')
            END", connection);
        
        await createCategoriesCommand.ExecuteNonQueryAsync();

        // Create Expenses table
        var createExpensesCommand = new SqlCommand(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Expenses' AND xtype='U')
            BEGIN
                CREATE TABLE Expenses (
                    Id int IDENTITY(1,1) PRIMARY KEY,
                    Amount decimal(10,2) NOT NULL,
                    CategoryId int NOT NULL,
                    Description nvarchar(500),
                    ExpenseDate datetime2 NOT NULL,
                    CreatedAt datetime2 DEFAULT GETDATE(),
                    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
                )
            END", connection);
        
        await createExpensesCommand.ExecuteNonQueryAsync();
    }
}