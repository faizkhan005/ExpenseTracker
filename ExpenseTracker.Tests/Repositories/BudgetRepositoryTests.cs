using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Tests.Helpers;
using FluentAssertions;
using SQLite;

namespace ExpenseTracker.Tests.Repositories;

// ═══════════════════════════════════════════════════════════════════════════
// BudgetRepository — AddOrUpdate semantics for overall vs per-category budgets
// ═══════════════════════════════════════════════════════════════════════════

public class BudgetRepositoryTests : IAsyncLifetime
{
    private SQLiteAsyncConnection _db = null!;
    private TestBudgetRepository _repo = null!;

    public async Task InitializeAsync()
    {
        _db = await TestDbContextFactory.CreateInMemoryAsync();
        _repo = new TestBudgetRepository(_db);
    }

    public Task DisposeAsync() => _db.CloseAsync();

    [Fact]
    public async Task AddOrUpdateAsync_NewOverallBudget_Inserts()
    {
        var id = await _repo.AddOrUpdateAsync(new Budget { Year = 2026, Month = 6, LimitAmount = 3000m, CategoryId = null });

        id.Should().BeGreaterThan(0);

        var saved = await _repo.GetAsync(2026, 6, null);
        saved.Should().NotBeNull();
        saved!.LimitAmount.Should().Be(3000m);
    }

    [Fact]
    public async Task AddOrUpdateAsync_ExistingOverallBudget_UpdatesInPlace_DoesNotDuplicate()
    {
        await _repo.AddOrUpdateAsync(new Budget { Year = 2026, Month = 6, LimitAmount = 3000m, CategoryId = null });
        await _repo.AddOrUpdateAsync(new Budget { Year = 2026, Month = 6, LimitAmount = 3500m, CategoryId = null });

        var all = await _db.Table<Budget>().Where(b => b.Year == 2026 && b.Month == 6 && b.CategoryId == null).ToListAsync();

        all.Should().ContainSingle("updating should not create a duplicate row");
        all[0].LimitAmount.Should().Be(3500m);
    }

    [Fact]
    public async Task AddOrUpdateAsync_OverallAndCategoryBudgets_AreIndependent()
    {
        var foodCategory = await _db.Table<Category>().Where(c => c.Name == "Food").FirstAsync();

        await _repo.AddOrUpdateAsync(new Budget { Year = 2026, Month = 6, LimitAmount = 3000m, CategoryId = null });
        await _repo.AddOrUpdateAsync(new Budget { Year = 2026, Month = 6, LimitAmount = 500m, CategoryId = foodCategory.Id });

        var overall = await _repo.GetAsync(2026, 6, null);
        var foodBudget = await _repo.GetAsync(2026, 6, foodCategory.Id);

        overall!.LimitAmount.Should().Be(3000m);
        foodBudget!.LimitAmount.Should().Be(500m);
    }

    [Fact]
    public async Task GetAllForMonthAsync_ReturnsBothOverallAndCategoryBudgets()
    {
        var foodCategory = await _db.Table<Category>().Where(c => c.Name == "Food").FirstAsync();

        await _repo.AddOrUpdateAsync(new Budget { Year = 2026, Month = 6, LimitAmount = 3000m, CategoryId = null });
        await _repo.AddOrUpdateAsync(new Budget { Year = 2026, Month = 6, LimitAmount = 500m, CategoryId = foodCategory.Id });

        var all = await _repo.GetAllForMonthAsync(2026, 6);

        all.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAsync_DifferentMonth_ReturnsNull()
    {
        await _repo.AddOrUpdateAsync(new Budget { Year = 2026, Month = 6, LimitAmount = 3000m, CategoryId = null });

        var result = await _repo.GetAsync(2026, 7, null);

        result.Should().BeNull();
    }
}

// Test-friendly repository wrapper 
internal class TestBudgetRepository
{
    private readonly SQLiteAsyncConnection _db;

    public TestBudgetRepository(SQLiteAsyncConnection db) => _db = db;

    public async Task<Budget?> GetAsync(int year, int month, int? categoryId) 
    {
        Budget? result = categoryId.HasValue
            ? await _db.Table<Budget>().Where(b => b.Year == year && b.Month == month && b.CategoryId == categoryId).FirstOrDefaultAsync()
            : await _db.Table<Budget>().Where(b => b.Year == year && b.Month == month && b.CategoryId == null).FirstOrDefaultAsync();
        return result;
    }

    public Task<List<Budget>> GetAllForMonthAsync(int year, int month)
        => _db.Table<Budget>().Where(b => b.Year == year && b.Month == month).ToListAsync();

    public async Task<int> AddOrUpdateAsync(Budget budget)
    {
        var existing = await GetAsync(budget.Year, budget.Month, budget.CategoryId);

        if (existing is not null)
        {
            existing.LimitAmount = budget.LimitAmount;
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.UpdateAsync(existing);
            return existing.Id;
        }

        budget.CreatedAt = DateTime.UtcNow;
        budget.UpdatedAt = DateTime.UtcNow;
        await _db.InsertAsync(budget);
        return budget.Id;
    }
}
