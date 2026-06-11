using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Infrastructure.Data.Repositories;

public class DeleteDBRepository : IDeleteDBRepository
{
    private readonly AppDbContext _dbContext;
    public DeleteDBRepository(AppDbContext context) => _dbContext = context;
    public async Task ClearAllDataAsync()
    {
        var db = await _dbContext.GetConnectionAsync();

        // Delete all data in reverse dependency order
        await db.DeleteAllAsync<LineItem>();
        await db.DeleteAllAsync<Expense>();
        await db.DeleteAllAsync<RecurringExpense>();
        await db.DeleteAllAsync<Budget>();
        await db.DeleteAllAsync<SmsRule>();
        await db.DeleteAllAsync<SavedLocation>();

        // Delete non-system categories only
        var customCats = await db.Table<Category>()
            .Where(c => !c.IsSystem)
            .ToListAsync();
        foreach (var cat in customCats)
            await db.DeleteAsync(cat);
    }
}
