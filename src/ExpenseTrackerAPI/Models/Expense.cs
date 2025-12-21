using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackerAPI.Models;

public class Expense
{
    public int Id { get; set; }
    
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
    
    public int CategoryId { get; set; }
    
    public string? Description { get; set; }
    
    [DataType(DataType.DateTime)]
    public DateTime ExpenseDate { get; set; }
    
    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public string? CategoryName { get; set; }
}

public class CreateExpenseRequest
{
    [Range(0.01, double.MaxValue)]
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    [System.Text.Json.Serialization.JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    [System.Text.Json.Serialization.JsonPropertyName("categoryId")]
    public int CategoryId { get; set; }
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    [System.Text.Json.Serialization.JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [DataType(DataType.DateTime)]
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    [System.Text.Json.Serialization.JsonPropertyName("expenseDate")]
    public DateTime ExpenseDate { get; set; }
}
