using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces;

public interface IBudgetRepository
{
    Task<Budget?> GetAsync(int year, int month, int? categoryId = null);
    Task<List<Budget>> GetAllForMonthAsync(int year, int month);
    Task<int> AddOrUpdateAsync(Budget budget);
    Task DeleteAsync(int id);
}
