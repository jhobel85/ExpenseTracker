-- Expense Tracker Database Schema
-- Run this script on your database

-- Categories table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE name='Categories' AND type='U')
BEGIN
    CREATE TABLE Categories (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL UNIQUE,
        Description NVARCHAR(200),
        CreatedAt DATETIME DEFAULT GETDATE()
    );
END

-- Expenses table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE name='Expenses' AND type='U')
BEGIN
    CREATE TABLE Expenses (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Amount DECIMAL(10,2) NOT NULL,
        CategoryId INT NOT NULL,
        Description NVARCHAR(500),
        ExpenseDate DATE NOT NULL,
        CreatedAt DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
    );
END

-- Insert default categories if not exists
IF NOT EXISTS (SELECT * FROM Categories WHERE Name = 'Food')
BEGIN
    INSERT INTO Categories (Name, Description) VALUES
    ('Food', 'Groceries, restaurants, dining'),
    ('Transport', 'Gas, public transport, parking'),
    ('Bills', 'Utilities, rent, subscriptions'),
    ('Entertainment', 'Movies, games, hobbies'),
    ('Healthcare', 'Medical, pharmacy, insurance'),
    ('Shopping', 'Clothing, electronics, misc'),
    ('Other', 'Uncategorized expenses');
END

-- View for monthly expense summary
GO
DROP VIEW IF EXISTS vw_MonthlySummary;
GO
CREATE VIEW vw_MonthlySummary AS
SELECT 
    c.Name AS Category,
    COUNT(e.Id) AS TransactionCount,
    SUM(e.Amount) AS TotalAmount,
    YEAR(e.ExpenseDate) AS Year,
    MONTH(e.ExpenseDate) AS Month
FROM Expenses e
INNER JOIN Categories c ON e.CategoryId = c.Id
GROUP BY c.Name, YEAR(e.ExpenseDate), MONTH(e.ExpenseDate);
GO

-- Stored Procedures
CREATE OR ALTER PROCEDURE GetCategories
AS
SELECT Id, Name, Description, CreatedAt FROM Categories ORDER BY Name
GO

CREATE OR ALTER PROCEDURE GetExpenses
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL
AS
SELECT e.Id, e.Amount, e.CategoryId, e.Description, e.ExpenseDate, e.CreatedAt, c.Name as CategoryName
FROM Expenses e
INNER JOIN Categories c ON e.CategoryId = c.Id
WHERE (@StartDate IS NULL OR e.ExpenseDate >= @StartDate)
  AND (@EndDate IS NULL OR e.ExpenseDate <= @EndDate)
ORDER BY e.ExpenseDate DESC
GO

CREATE OR ALTER PROCEDURE GetExpenseById
    @Id INT
AS
SELECT e.Id, e.Amount, e.CategoryId, e.Description, e.ExpenseDate, e.CreatedAt, c.Name as CategoryName
FROM Expenses e
INNER JOIN Categories c ON e.CategoryId = c.Id
WHERE e.Id = @Id
GO

CREATE OR ALTER PROCEDURE CreateExpense
    @Amount DECIMAL(10,2),
    @CategoryId INT,
    @Description NVARCHAR(500),
    @ExpenseDate DATE
AS
INSERT INTO Expenses (Amount, CategoryId, Description, ExpenseDate) 
VALUES (@Amount, @CategoryId, @Description, @ExpenseDate);
SELECT CAST(SCOPE_IDENTITY() as int);
GO

CREATE OR ALTER PROCEDURE DeleteExpense
    @Id INT
AS
DELETE FROM Expenses WHERE Id = @Id
GO

CREATE OR ALTER PROCEDURE GetMonthlySummary
    @Year INT,
    @Month INT
AS
SELECT c.Name, ISNULL(SUM(e.Amount), 0) as Total
FROM Categories c
LEFT JOIN Expenses e ON c.Id = e.CategoryId 
  AND YEAR(e.ExpenseDate) = @Year 
  AND MONTH(e.ExpenseDate) = @Month
GROUP BY c.Name
ORDER BY Total DESC
GO
