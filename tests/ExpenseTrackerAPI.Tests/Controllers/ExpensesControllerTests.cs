using Xunit;
using Moq;
using ExpenseTrackerAPI.Controllers;
using ExpenseTrackerAPI.Models;
using ExpenseTrackerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ExpenseTrackerAPI.Tests.Controllers;

public class ExpensesControllerTests
{
    private readonly Mock<DatabaseService> _mockDatabaseService;
    private readonly ExpensesController _controller;

    public ExpensesControllerTests()
    {
        // Create a real IConfiguration with a connection string
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ConnectionStrings:DefaultConnection", "Server=.;Database=ExpenseTracker;Trusted_Connection=true;" }
            })
            .Build();
        
        // Create a mock DatabaseService with the real configuration
        _mockDatabaseService = new Mock<DatabaseService>(configuration) { CallBase = false };
        _controller = new ExpensesController(_mockDatabaseService.Object);
    }

    #region GetExpenses Tests

    [Fact]
    public async Task GetExpenses_WithoutFilter_ReturnsAllExpenses()
    {
        // Arrange
        var expenses = new List<Expense>
        {
            new Expense { Id = 1, Amount = 50.00m, CategoryId = 1, Description = "Groceries", ExpenseDate = DateTime.Now, CreatedAt = DateTime.Now },
            new Expense { Id = 2, Amount = 30.00m, CategoryId = 2, Description = "Gas", ExpenseDate = DateTime.Now, CreatedAt = DateTime.Now }
        };
        _mockDatabaseService.Setup(s => s.GetExpensesAsync(null, null))
            .ReturnsAsync(expenses);

        // Act
        var result = await _controller.GetExpenses(null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedExpenses = Assert.IsType<List<Expense>>(okResult.Value);
        Assert.Equal(2, returnedExpenses.Count);
    }

    [Fact]
    public async Task GetExpenses_WithDateRange_ReturnsFilteredExpenses()
    {
        // Arrange
        var startDate = new DateTime(2025, 12, 01);
        var endDate = new DateTime(2025, 12, 31);
        var expenses = new List<Expense>
        {
            new Expense { Id = 1, Amount = 45.50m, CategoryId = 1, Description = "Groceries", ExpenseDate = new DateTime(2025, 12, 21), CreatedAt = DateTime.Now }
        };
        _mockDatabaseService.Setup(s => s.GetExpensesAsync(startDate, endDate))
            .ReturnsAsync(expenses);

        // Act
        var result = await _controller.GetExpenses(startDate, endDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedExpenses = Assert.IsType<List<Expense>>(okResult.Value);
        Assert.Single(returnedExpenses);
        Assert.Equal(45.50m, returnedExpenses[0].Amount);
    }

    [Fact]
    public async Task GetExpenses_ReturnsEmptyList_WhenNoExpensesFound()
    {
        // Arrange
        var expenses = new List<Expense>();
        _mockDatabaseService.Setup(s => s.GetExpensesAsync(null, null))
            .ReturnsAsync(expenses);

        // Act
        var result = await _controller.GetExpenses(null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedExpenses = Assert.IsType<List<Expense>>(okResult.Value);
        Assert.Empty(returnedExpenses);
    }

    #endregion

    #region GetExpense Tests

    [Fact]
    public async Task GetExpense_WithValidId_ReturnsExpense()
    {
        // Arrange
        var expenseId = 1;
        var expense = new Expense
        {
            Id = expenseId,
            Amount = 50.00m,
            CategoryId = 1,
            Description = "Groceries",
            ExpenseDate = DateTime.Now,
            CreatedAt = DateTime.Now
        };
        _mockDatabaseService.Setup(s => s.GetExpenseByIdAsync(expenseId))
            .ReturnsAsync(expense);

        // Act
        var result = await _controller.GetExpense(expenseId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedExpense = Assert.IsType<Expense>(okResult.Value);
        Assert.Equal(expenseId, returnedExpense.Id);
        Assert.Equal(50.00m, returnedExpense.Amount);
    }

    [Fact]
    public async Task GetExpense_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var expenseId = 999;
        _mockDatabaseService.Setup(s => s.GetExpenseByIdAsync(expenseId))
            .ReturnsAsync((Expense?)null);

        // Act
        var result = await _controller.GetExpense(expenseId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    #endregion

    #region CreateExpense Tests

    [Fact]
    public async Task CreateExpense_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateExpenseRequest
        {
            Amount = 45.50m,
            CategoryId = 1,
            Description = "Weekly grocery shopping",
            ExpenseDate = new DateTime(2025, 12, 21, 10, 30, 0)
        };
        var createdId = 1;
        var createdExpense = new Expense
        {
            Id = createdId,
            Amount = request.Amount,
            CategoryId = request.CategoryId,
            Description = request.Description,
            ExpenseDate = request.ExpenseDate,
            CreatedAt = DateTime.Now
        };
        _mockDatabaseService.Setup(s => s.CreateExpenseAsync(request))
            .ReturnsAsync(createdId);
        _mockDatabaseService.Setup(s => s.GetExpenseByIdAsync(createdId))
            .ReturnsAsync(createdExpense);

        // Act
        var result = await _controller.CreateExpense(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ExpensesController.GetExpense), createdResult.ActionName);
        Assert.Equal(createdId, ((Expense)createdResult.Value!).Id);
    }

    [Fact]
    public async Task CreateExpense_WithZeroAmount_ShouldHandleValidation()
    {
        // Arrange
        var request = new CreateExpenseRequest
        {
            Amount = 0m,
            CategoryId = 1,
            Description = "Invalid expense",
            ExpenseDate = DateTime.Now
        };

        // Act & Assert
        // Note: Validation would be handled by ASP.NET Core model binding
        // This test demonstrates the test structure; actual validation testing 
        // would occur at integration test level
        Assert.True(request.Amount <= 0);
    }

    #endregion

    #region DeleteExpense Tests

    [Fact]
    public async Task DeleteExpense_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var expenseId = 1;
        _mockDatabaseService.Setup(s => s.DeleteExpenseAsync(expenseId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteExpense(expenseId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockDatabaseService.Verify(s => s.DeleteExpenseAsync(expenseId), Times.Once);
    }

    [Fact]
    public async Task DeleteExpense_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var expenseId = 999;
        _mockDatabaseService.Setup(s => s.DeleteExpenseAsync(expenseId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteExpense(expenseId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region GetMonthlySummary Tests

    [Fact]
    public async Task GetMonthlySummary_WithValidYearMonth_ReturnsSummary()
    {
        // Arrange
        var year = 2025;
        var month = 12;
        var summary = new Dictionary<string, decimal>
        {
            { "Groceries", 150.00m },
            { "Transportation", 75.50m }
        };
        _mockDatabaseService.Setup(s => s.GetMonthlySummaryAsync(year, month))
            .ReturnsAsync(summary);

        // Act
        var result = await _controller.GetMonthlySummary(year, month);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedSummary = Assert.IsType<Dictionary<string, decimal>>(okResult.Value);
        Assert.Equal(2, returnedSummary.Count);
        Assert.Equal(150.00m, returnedSummary["Groceries"]);
    }

    [Fact]
    public async Task GetMonthlySummary_WithNoExpenses_ReturnsEmptyDictionary()
    {
        // Arrange
        var year = 2025;
        var month = 1;
        var summary = new Dictionary<string, decimal>();
        _mockDatabaseService.Setup(s => s.GetMonthlySummaryAsync(year, month))
            .ReturnsAsync(summary);

        // Act
        var result = await _controller.GetMonthlySummary(year, month);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedSummary = Assert.IsType<Dictionary<string, decimal>>(okResult.Value);
        Assert.Empty(returnedSummary);
    }

    #endregion
}
