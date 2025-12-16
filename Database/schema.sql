-- Expense Tracker Database Schema
-- Run this script on your AWS RDS MS SQL database

-- Categories table
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Expenses table
CREATE TABLE Expenses (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Amount DECIMAL(10,2) NOT NULL,
    CategoryId INT NOT NULL,
    Description NVARCHAR(500),
    ExpenseDate DATE NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

-- Insert default categories
INSERT INTO Categories (Name, Description) VALUES
('Food', 'Groceries, restaurants, dining'),
('Transport', 'Gas, public transport, parking'),
('Bills', 'Utilities, rent, subscriptions'),
('Entertainment', 'Movies, games, hobbies'),
('Healthcare', 'Medical, pharmacy, insurance'),
('Shopping', 'Clothing, electronics, misc'),
('Other', 'Uncategorized expenses');

-- View for monthly expense summary
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
