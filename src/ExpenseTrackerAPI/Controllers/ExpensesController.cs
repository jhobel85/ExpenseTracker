using Microsoft.AspNetCore.Mvc;
using ExpenseTrackerAPI.Models;
using ExpenseTrackerAPI.Services;

namespace ExpenseTrackerAPI.Controllers;

/// <summary>
/// Controller for managing expenses with filtering by date range
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly DatabaseService _dbService;

    public ExpensesController(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    /// <summary>
    /// Get all expenses with optional date filtering
    /// </summary>
    /// <param name="startDate">Optional start date in ISO 8601 format (yyyy-MM-ddTHH:mm:ss.fffZ), e.g., 2025-01-01T00:00:00.000Z</param>
    /// <param name="endDate">Optional end date in ISO 8601 format (yyyy-MM-ddTHH:mm:ss.fffZ), e.g., 2025-12-31T23:59:59.999Z</param>
    /// <returns>List of expenses matching the criteria</returns>
    [HttpGet]
    public async Task<ActionResult<List<Expense>>> GetExpenses(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var expenses = await _dbService.GetExpensesAsync(startDate, endDate);
        return Ok(expenses);
    }

    /// <summary>
    /// Get a specific expense by ID
    /// </summary>
    /// <param name="id">The expense ID</param>
    /// <returns>The expense details including ExpenseDate and CreatedAt in ISO 8601 format</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Expense>> GetExpense(int id)
    {
        var expense = await _dbService.GetExpenseByIdAsync(id);
        if (expense == null)
            return NotFound();

        return Ok(expense);
    }

    /// <summary>
    /// Create a new expense
    /// </summary>
    /// <param name="request">Expense data including ExpenseDate in ISO 8601 format (yyyy-MM-ddTHH:mm:ss.fffZ)</param>
    /// <returns>The newly created expense with generated ID and timestamps</returns>
    [HttpPost]
    public async Task<ActionResult<Expense>> CreateExpense(CreateExpenseRequest request)
    {
        var id = await _dbService.CreateExpenseAsync(request);
        var expense = await _dbService.GetExpenseByIdAsync(id);
        return CreatedAtAction(nameof(GetExpense), new { id }, expense);
    }

    /// <summary>
    /// Delete an expense by ID
    /// </summary>
    /// <param name="id">The expense ID to delete</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var deleted = await _dbService.DeleteExpenseAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Get monthly expense summary
    /// </summary>
    /// <param name="year">The year (e.g., 2025)</param>
    /// <param name="month">The month (1-12)</param>
    /// <returns>Dictionary with category names and total amounts for the month</returns>
    [HttpGet("summary/{year}/{month}")]
    public async Task<ActionResult<Dictionary<string, decimal>>> GetMonthlySummary(int year, int month)
    {
        var summary = await _dbService.GetMonthlySummaryAsync(year, month);
        return Ok(summary);
    }
}
