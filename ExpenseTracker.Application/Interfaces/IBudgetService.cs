using ExpenseTracker.Application.DTO;

namespace ExpenseTracker.Application.Interfaces;

public interface IBudgetService
{

    Task<decimal> GetMonthlyBudgetAsync(int year, int month);
    Task SetMonthlyBudgetAsync(int year, int month, decimal amount);
    Task<decimal> GetCategoryBudgetAsync(int year, int month, int categoryId);
    Task SetCategoryBudgetAsync(int year, int month, int categoryId, decimal amount);
    Task<BudgetSummary> GetBudgetSummaryAsync(int year, int month);
}
