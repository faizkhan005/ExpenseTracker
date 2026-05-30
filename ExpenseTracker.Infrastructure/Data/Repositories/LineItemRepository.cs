using ExpenseTracker.Application.DTO;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Infrastructure.Data.Repositories;

public class LineItemRepository : ILineItemRepository
{
    private readonly AppDbContext _context;

    public LineItemRepository(AppDbContext context) => _context = context;

    public async Task<List<LineItem>> GetByExpenseIdAsync(int expenseId)
    {
        var db = await _context.GetConnectionAsync();
        return await db.Table<LineItem>().Where(l => l.ExpenseId == expenseId).ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<LineItem> items)
    {
        var db = await _context.GetConnectionAsync();
        var list = items.ToList();
        var now = DateTime.UtcNow;
        foreach (var item in list) { item.CreatedAt = now; item.UpdatedAt = now; }
        await db.InsertAllAsync(list);
    }

    public async Task DeleteByExpenseIdAsync(int expenseId)
    {
        var db = await _context.GetConnectionAsync();
        await db.Table<LineItem>().DeleteAsync(l => l.ExpenseId == expenseId);
    }

    public async Task<List<LineItemHistory>> GetPurchaseHistoryAsync(string productName, int monthsBack = 3)
    {
        var db = await _context.GetConnectionAsync();
        var cutoff = DateTime.Today.AddMonths(-monthsBack);

        // Join LineItems → Expenses to filter by date
        var allItems = await db.Table<LineItem>()
            .Where(l => l.Name.Contains(productName))
            .ToListAsync();

        var expenseIds = allItems.Select(l => l.ExpenseId).Distinct().ToList();
        var expenses = await db.Table<Expense>()
            .Where(e => expenseIds.Contains(e.Id) && e.Date >= cutoff)
            .ToListAsync();

        var expenseDateMap = expenses.ToDictionary(e => e.Id, e => e.Date);

        return [.. allItems
            .Where(l => expenseDateMap.ContainsKey(l.ExpenseId))
            .Select(l => new LineItemHistory
            {
                ProductName = l.Name,
                Quantity = l.Quantity,
                PurchaseDate = expenseDateMap[l.ExpenseId]
            })
            .OrderByDescending(h => h.PurchaseDate)];
    }
}
