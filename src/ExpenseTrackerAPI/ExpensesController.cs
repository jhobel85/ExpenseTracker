using Microsoft.AspNetCore.Mvc;
using ExpenseTrackerAPI.Models;
using ExpenseTrackerAPI.Services;

namespace ExpenseTrackerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly DatabaseService _dbService;

    public ExpensesController(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Expense>>> GetExpenses(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var expenses = await _dbService.GetExpensesAsync(startDate, endDate);
        return Ok(expenses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Expense>> GetExpense(int id)
    {
        var expense = await _dbService.GetExpenseByIdAsync(id);
        if (expense == null)
            return NotFound();

        return Ok(expense);
    }

    [HttpPost]
    public async Task<ActionResult<Expense>> CreateExpense(CreateExpenseRequest request)
    {
        var id = await _dbService.CreateExpenseAsync(request);
        var expense = await _dbService.GetExpenseByIdAsync(id);
        return CreatedAtAction(nameof(GetExpense), new { id }, expense);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var deleted = await _dbService.DeleteExpenseAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("summary/{year}/{month}")]
    public async Task<ActionResult<Dictionary<string, decimal>>> GetMonthlySummary(int year, int month)
    {
        var summary = await _dbService.GetMonthlySummaryAsync(year, month);
        return Ok(summary);
    }
}
