using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using SQLite;

namespace ExpenseTracker.Infrastructure.Data.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly AppDbContext _context;

    public ExpenseRepository(AppDbContext context) => _context = context;

    public async Task<List<Expense>> GetAllAsync()
    {
        var db = await _context.GetConnectionAsync();
        var expenses = await db.Table<Expense>().OrderByDescending(e => e.Date).ToListAsync();
        await PopulateCategoriesAsync(db, expenses);
        return expenses;
    }

    public async Task<List<Expense>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var db = await _context.GetConnectionAsync();
        var expenses = await db.Table<Expense>()
            .Where(e => e.Date >= from && e.Date <= to)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
        await PopulateCategoriesAsync(db, expenses);
        return expenses;
    }

    public async Task<List<Expense>> GetByCategoryAsync(int categoryId, DateTime from, DateTime to)
    {
        var db = await _context.GetConnectionAsync();
        var expenses = await db.Table<Expense>()
            .Where(e => e.CategoryId == categoryId && e.Date >= from && e.Date <= to)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
        await PopulateCategoriesAsync(db, expenses);
        return expenses;
    }

    public async Task<List<Expense>> GetBySourceAsync(ExpenseSource source)
    {
        var db = await _context.GetConnectionAsync();
        var expenses = await db.Table<Expense>()
            .Where(e => e.Source == source)
            .ToListAsync();
        await PopulateCategoriesAsync(db, expenses);
        return expenses;
    }

    public async Task<Expense?> GetByIdAsync(int id)
    {
        var db = await _context.GetConnectionAsync();
        var expense = await db.Table<Expense>().Where(e => e.Id == id).FirstOrDefaultAsync();
        if (expense is null) return null;

        expense.Category = await db.Table<Category>().Where(c => c.Id == expense.CategoryId).FirstOrDefaultAsync();
        expense.LineItems = await db.Table<LineItem>().Where(l => l.ExpenseId == id).ToListAsync();
        return expense;
    }


    public async Task<int> AddAsync(Expense expense)
    {
        var db = await _context.GetConnectionAsync();
        expense.CreatedAt = DateTime.UtcNow;
        expense.UpdatedAt = DateTime.UtcNow;
        await db.InsertAsync(expense);
        return expense.Id;
    }

    public async Task UpdateAsync(Expense expense)
    {
        var db = await _context.GetConnectionAsync();
        expense.UpdatedAt = DateTime.UtcNow;
        await db.UpdateAsync(expense);
    }

    public async Task DeleteAsync(int id)
    {
        var db = await _context.GetConnectionAsync();
        await db.DeleteAsync<Expense>(id);
        // Cascade delete line items
        await db.Table<LineItem>().DeleteAsync(l => l.ExpenseId == id);
    }

    public async Task<decimal> GetTotalAsync(DateTime from, DateTime to, TransactionType type = TransactionType.Expense)
    {
        var db = await _context.GetConnectionAsync();
        var expenses = await db.Table<Expense>()
            .Where(e => e.Date >= from && e.Date <= to && e.Type == type)
            .ToListAsync();
        return expenses.Sum(e => e.Amount);
    }

    public async Task<decimal> GetTotalByCategoryAsync(int categoryId, DateTime from, DateTime to)
    {
        var db = await _context.GetConnectionAsync();
        var expenses = await db.Table<Expense>()
            .Where(e => e.CategoryId == categoryId && e.Date >= from && e.Date <= to && e.Type == TransactionType.Expense)
            .ToListAsync();
        return expenses.Sum(e => e.Amount);
    }

    private static async Task PopulateCategoriesAsync(SQLiteAsyncConnection db, List<Expense> expenses)
    {
        var categoryIds = expenses.Select(e => e.CategoryId).Distinct().ToList();
        if (!categoryIds.Any()) return;

        var categories = await db.Table<Category>()
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync();

        var categoryMap = categories.ToDictionary(c => c.Id);

        foreach (var expense in expenses)
            expense.Category = categoryMap.GetValueOrDefault(expense.CategoryId);
    }
}
