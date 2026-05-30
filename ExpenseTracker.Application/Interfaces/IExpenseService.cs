using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces;

public interface IExpenseService
{
    Task<List<Expense>> GetExpensesAsync(DateTime from, DateTime to);
    Task<List<Expense>> GetExpensesByCategoryAsync(int categoryId, DateTime from, DateTime to);
    Task<Expense?> GetExpenseByIdAsync(int id);
    Task<int> AddExpenseAsync(Expense expense, List<LineItem>? lineItems = null);
    Task UpdateExpenseAsync(Expense expense);
    Task DeleteExpenseAsync(int id);
    Task<decimal> GetTotalSpentAsync(int year, int month);
    Task<decimal> GetTotalIncomeAsync(int year, int month);
    Task<decimal> GetSavingsAsync(int year, int month);
    Task<decimal> GetAverageDailySpendAsync(int year, int month);
    Task<Dictionary<string, decimal>> GetSpendingByCategoryAsync(int year, int month);
}
