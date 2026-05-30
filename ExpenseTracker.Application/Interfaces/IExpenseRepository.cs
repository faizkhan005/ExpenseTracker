using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Interfaces;

public interface IExpenseRepository
{
    Task<List<Expense>> GetAllAsync();
    Task<List<Expense>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<List<Expense>> GetByCategoryAsync(int categoryId, DateTime from, DateTime to);
    Task<List<Expense>> GetBySourceAsync(ExpenseSource source);
    Task<Expense?> GetByIdAsync(int id);
    Task<int> AddAsync(Expense expense);
    Task UpdateAsync(Expense expense);
    Task DeleteAsync(int id);
    Task<decimal> GetTotalAsync(DateTime from, DateTime to, TransactionType type = TransactionType.Expense);
    Task<decimal> GetTotalByCategoryAsync(int categoryId, DateTime from, DateTime to);
}
