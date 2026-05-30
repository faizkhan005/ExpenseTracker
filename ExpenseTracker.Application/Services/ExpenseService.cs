using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepo;
    private readonly ILineItemRepository _lineItemRepo;

    public ExpenseService(IExpenseRepository expenseRepo, ILineItemRepository lineItemRepo)
    {
        _expenseRepo = expenseRepo;
        _lineItemRepo = lineItemRepo;
    }

    public Task<List<Expense>> GetExpensesAsync(DateTime from, DateTime to)
        => _expenseRepo.GetByDateRangeAsync(from, to);

    public Task<List<Expense>> GetExpensesByCategoryAsync(int categoryId, DateTime from, DateTime to)
        => _expenseRepo.GetByCategoryAsync(categoryId, from, to);

    public Task<Expense?> GetExpenseByIdAsync(int id)
        => _expenseRepo.GetByIdAsync(id);

    public async Task<int> AddExpenseAsync(Expense expense, List<LineItem>? lineItems = null)
    {
        var id = await _expenseRepo.AddAsync(expense);

        if (lineItems is { Count: > 0 })
        {
            foreach (var item in lineItems)
                item.ExpenseId = id;
            await _lineItemRepo.AddRangeAsync(lineItems);
        }

        return id;
    }

    public Task UpdateExpenseAsync(Expense expense)
        => _expenseRepo.UpdateAsync(expense);

    public Task DeleteExpenseAsync(int id)
        => _expenseRepo.DeleteAsync(id);

    public async Task<decimal> GetTotalSpentAsync(int year, int month)
    {
        var (from, to) = GetMonthRange(year, month);
        return await _expenseRepo.GetTotalAsync(from, to, TransactionType.Expense);
    }

    public async Task<decimal> GetTotalIncomeAsync(int year, int month)
    {
        var (from, to) = GetMonthRange(year, month);
        return await _expenseRepo.GetTotalAsync(from, to, TransactionType.Income);
    }

    public async Task<decimal> GetSavingsAsync(int year, int month)
    {
        var income = await GetTotalIncomeAsync(year, month);
        var spent = await GetTotalSpentAsync(year, month);
        return income - spent;
    }

    public async Task<decimal> GetAverageDailySpendAsync(int year, int month)
    {
        var (from, to) = GetMonthRange(year, month);
        var total = await _expenseRepo.GetTotalAsync(from, to, TransactionType.Expense);
        var daysElapsed = (DateTime.Today - from).Days + 1;
        return daysElapsed > 0 ? total / daysElapsed : 0;
    }

    public async Task<Dictionary<string, decimal>> GetSpendingByCategoryAsync(int year, int month)
    {
        var (from, to) = GetMonthRange(year, month);
        var expenses = await _expenseRepo.GetByDateRangeAsync(from, to);
        return expenses
            .Where(e => e.Type == TransactionType.Expense)
            .GroupBy(e => e.Category?.Name ?? "Other")
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
    }

    private static (DateTime from, DateTime to) GetMonthRange(int year, int month)
    {
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddSeconds(-1);
        return (from, to);
    }
}
