using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces;

public interface IRecurringExpenseRepository
{
    Task<List<RecurringExpense>> GetAllActiveAsync();
    Task<RecurringExpense?> GetByIdAsync(int id);
    Task<int> AddAsync(RecurringExpense rule);
    Task UpdateAsync(RecurringExpense rule);
    Task DeleteAsync(int id);

    /// <summary>Returns rules that are due to fire today.</summary>
    Task<List<RecurringExpense>> GetDueTodayAsync();
}
