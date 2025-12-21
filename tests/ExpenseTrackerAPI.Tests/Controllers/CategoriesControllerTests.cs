using Xunit;
using Moq;
using ExpenseTrackerAPI.Controllers;
using ExpenseTrackerAPI.Models;
using ExpenseTrackerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ExpenseTrackerAPI.Tests.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<DatabaseService> _mockDatabaseService;
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
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
        _controller = new CategoriesController(_mockDatabaseService.Object);
    }

    [Fact]
    public async Task GetCategories_ReturnsAllCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Groceries", Description = "Food and grocery shopping", CreatedAt = DateTime.Now },
            new Category { Id = 2, Name = "Transportation", Description = "Car gas and transportation", CreatedAt = DateTime.Now },
            new Category { Id = 3, Name = "Entertainment", Description = "Movies, games, and entertainment", CreatedAt = DateTime.Now }
        };
        _mockDatabaseService.Setup(s => s.GetCategoriesAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _controller.GetCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCategories = Assert.IsType<List<Category>>(okResult.Value);
        Assert.Equal(3, returnedCategories.Count);
        Assert.Equal("Groceries", returnedCategories[0].Name);
    }

    [Fact]
    public async Task GetCategories_ReturnsEmptyList_WhenNoCategoriesExist()
    {
        // Arrange
        var categories = new List<Category>();
        _mockDatabaseService.Setup(s => s.GetCategoriesAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _controller.GetCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCategories = Assert.IsType<List<Category>>(okResult.Value);
        Assert.Empty(returnedCategories);
    }

    [Fact]
    public async Task GetCategories_CallsDatabaseService_Exactly_Once()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Groceries", Description = "Food shopping", CreatedAt = DateTime.Now }
        };
        _mockDatabaseService.Setup(s => s.GetCategoriesAsync())
            .ReturnsAsync(categories);

        // Act
        await _controller.GetCategories();

        // Assert
        _mockDatabaseService.Verify(s => s.GetCategoriesAsync(), Times.Once);
    }
}
