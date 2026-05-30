using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.DTO;

public class CategoryBudgetLine
{
    public Category Category { get; set; } = null!;
    public decimal Budget { get; set; }
    public decimal Spent { get; set; }
    public decimal Remaining => Budget - Spent;
}
