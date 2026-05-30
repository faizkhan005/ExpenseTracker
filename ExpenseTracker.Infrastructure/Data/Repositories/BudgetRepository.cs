using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Infrastructure.Data.Repositories
{
    public class BudgetRepository : IBudgetRepository
    {
        private readonly AppDbContext _context;

        public BudgetRepository(AppDbContext context) => _context = context;

        public async Task<Budget?> GetAsync(int year, int month, int? categoryId = null)
        {
            var db = await _context.GetConnectionAsync();
            return categoryId.HasValue
                ? await db.Table<Budget>().Where(b => b.Year == year && b.Month == month && b.CategoryId == categoryId).FirstOrDefaultAsync()
                : await db.Table<Budget>().Where(b => b.Year == year && b.Month == month && b.CategoryId == null).FirstOrDefaultAsync();
        }

        public async Task<List<Budget>> GetAllForMonthAsync(int year, int month)
        {
            var db = await _context.GetConnectionAsync();
            return await db.Table<Budget>().Where(b => b.Year == year && b.Month == month).ToListAsync();
        }

        public async Task<int> AddOrUpdateAsync(Budget budget)
        {
            var db = await _context.GetConnectionAsync();
            var existing = await GetAsync(budget.Year, budget.Month, budget.CategoryId);

            if (existing is not null)
            {
                existing.LimitAmount = budget.LimitAmount;
                existing.UpdatedAt = DateTime.UtcNow;
                await db.UpdateAsync(existing);
                return existing.Id;
            }

            budget.CreatedAt = DateTime.UtcNow;
            budget.UpdatedAt = DateTime.UtcNow;
            await db.InsertAsync(budget);
            return budget.Id;
        }

        public async Task DeleteAsync(int id)
        {
            var db = await _context.GetConnectionAsync();
            await db.DeleteAsync<Budget>(id);
        }

    }
}
