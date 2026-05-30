using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces;

public interface IRecurringExpenseService
{
    Task<List<RecurringExpense>> GetAllAsync();
    Task<int> AddAsync(RecurringExpense rule);
    Task UpdateAsync(RecurringExpense rule);
    Task DeleteAsync(int id);

    /// <summary>
    /// Processes all rules due today and creates Expense records.
    /// Called on app startup and optionally by a background job.
    /// </summary>
    Task ProcessDueRecurringExpensesAsync();
}
