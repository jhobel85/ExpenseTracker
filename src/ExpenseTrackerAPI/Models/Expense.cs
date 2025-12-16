namespace ExpenseTrackerAPI.Models;

public class Expense
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public int CategoryId { get; set; }
    public string? Description { get; set; }
    public DateTime ExpenseDate { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public string? CategoryName { get; set; }
}

public class CreateExpenseRequest
{
    public decimal Amount { get; set; }
    public int CategoryId { get; set; }
    public string? Description { get; set; }
    public DateTime ExpenseDate { get; set; }
}
