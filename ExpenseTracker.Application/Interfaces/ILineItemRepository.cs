using ExpenseTracker.Application.DTO;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces;

public interface ILineItemRepository
{
    Task<List<LineItem>> GetByExpenseIdAsync(int expenseId);
    Task AddRangeAsync(IEnumerable<LineItem> items);
    Task DeleteByExpenseIdAsync(int expenseId);

    /// <summary>
    /// Returns aggregated purchase history for a product name across all expenses.
    /// Used for quantity recommendations.
    /// </summary>
    Task<List<LineItemHistory>> GetPurchaseHistoryAsync(string productName, int monthsBack = 3);
}
