namespace ExpenseTracker.Application.DTO;

public class BudgetSummary
{
    public decimal TotalBudget { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal Remaining => TotalBudget - TotalSpent;
    public double ProgressRatio => TotalBudget == 0 ? 0 : (double)(TotalSpent / TotalBudget);
    public bool IsOverBudget => TotalSpent > TotalBudget;
    public List<CategoryBudgetLine> CategoryLines { get; set; } = new();
}
