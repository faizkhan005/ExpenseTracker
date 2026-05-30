using ExpenseTracker.Models;

namespace ExpenseTracker.Services.Interface
{
    public interface IExpenseService
    {
        /// <summary>Returns all expenses (and optionally income) in the given date range.
        /// </summary>
        Task<List<Expense>> GetExpensesAsync(DateTime from, DateTime to);

        /// <summary>Returns total income logged for the given month.
        /// </summary>
        Task<decimal> GetIncomeAsync(int year, int month);

        Task AddExpenseAsync(Expense expense);
        Task UpdateExpenseAsync(Expense expense);
        Task DeleteExpenseAsync(int id);
    }
}
