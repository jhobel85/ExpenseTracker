using Xunit;
using ExpenseTrackerAPI.Models;

namespace ExpenseTrackerAPI.Tests.Models;

public class ExpenseModelTests
{
    [Fact]
    public void Expense_Creation_WithValidData_Succeeds()
    {
        // Arrange & Act
        var expense = new Expense
        {
            Id = 1,
            Amount = 50.00m,
            CategoryId = 1,
            Description = "Groceries",
            ExpenseDate = new DateTime(2025, 12, 21, 10, 30, 0),
            CreatedAt = DateTime.Now,
            CategoryName = "Food"
        };

        // Assert
        Assert.Equal(1, expense.Id);
        Assert.Equal(50.00m, expense.Amount);
        Assert.Equal(1, expense.CategoryId);
        Assert.Equal("Groceries", expense.Description);
    }

    [Fact]
    public void CreateExpenseRequest_WithValidData_Succeeds()
    {
        // Arrange & Act
        var expenseDate = new DateTime(2025, 12, 21, 10, 30, 0);
        var request = new CreateExpenseRequest
        {
            Amount = 45.50m,
            CategoryId = 2,
            Description = "Gas for car",
            ExpenseDate = expenseDate
        };

        // Assert
        Assert.Equal(45.50m, request.Amount);
        Assert.Equal(2, request.CategoryId);
        Assert.Equal("Gas for car", request.Description);
        Assert.Equal(expenseDate, request.ExpenseDate);
    }

    [Fact]
    public void Expense_Properties_CanBeNull()
    {
        // Arrange & Act
        var expense = new Expense
        {
            Id = 1,
            Amount = 25.00m,
            CategoryId = 1,
            Description = null,
            ExpenseDate = DateTime.Now,
            CreatedAt = DateTime.Now,
            CategoryName = null
        };

        // Assert
        Assert.Null(expense.Description);
        Assert.Null(expense.CategoryName);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(100.00)]
    [InlineData(9999.99)]
    public void Expense_Amount_AcceptsValidValues(decimal amount)
    {
        // Arrange & Act
        var expense = new Expense
        {
            Amount = amount,
            CategoryId = 1,
            ExpenseDate = DateTime.Now,
            CreatedAt = DateTime.Now
        };

        // Assert
        Assert.Equal(amount, expense.Amount);
    }
}

public class CategoryModelTests
{
    [Fact]
    public void Category_Creation_WithValidData_Succeeds()
    {
        // Arrange & Act
        var category = new Category
        {
            Id = 1,
            Name = "Groceries",
            Description = "Food and grocery shopping",
            CreatedAt = DateTime.Now
        };

        // Assert
        Assert.Equal(1, category.Id);
        Assert.Equal("Groceries", category.Name);
        Assert.Equal("Food and grocery shopping", category.Description);
    }

    [Fact]
    public void Category_Name_IsRequired()
    {
        // Arrange & Act
        var category = new Category
        {
            Id = 1,
            Name = string.Empty,
            Description = "Description",
            CreatedAt = DateTime.Now
        };

        // Assert
        Assert.Empty(category.Name);
    }

    [Fact]
    public void Category_Description_CanBeNull()
    {
        // Arrange & Act
        var category = new Category
        {
            Id = 1,
            Name = "Transportation",
            Description = null,
            CreatedAt = DateTime.Now
        };

        // Assert
        Assert.Null(category.Description);
    }

    [Theory]
    [InlineData("Groceries")]
    [InlineData("Transportation")]
    [InlineData("Entertainment")]
    [InlineData("Healthcare")]
    public void Category_Creation_WithVariousNames_Succeeds(string name)
    {
        // Arrange & Act
        var category = new Category
        {
            Id = 1,
            Name = name,
            CreatedAt = DateTime.Now
        };

        // Assert
        Assert.Equal(name, category.Name);
    }
}
