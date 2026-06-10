using ExpenseTracker.Application.DTO;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Models;

public class CategoryBudgetLineDisplay
{
    public CategoryBudgetLine Source { get; }
    public Category Category => Source.Category;
    public decimal Spent => Source.Spent;
    public decimal Budget => Source.Budget;
    public double ProgressRatio => Source.Budget > 0 ? (double)(Source.Spent / Source.Budget) : 0;
    public Color ProgressColor => ProgressRatio > 1 ? Color.FromArgb("#E24B4A") : Color.FromArgb("#534AB7");
    public string StatusLabel => Source.Budget > 0
        ? $"{Source.Remaining:C} remaining"
        : $"{Source.Spent:C} spent (no budget set)";
    public Color StatusColor => Source.Remaining >= 0 ? Color.FromArgb("#1D9E75") : Color.FromArgb("#E24B4A");

    public CategoryBudgetLineDisplay(CategoryBudgetLine source) => Source = source;
}
