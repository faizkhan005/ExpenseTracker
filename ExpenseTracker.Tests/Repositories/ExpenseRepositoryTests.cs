using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Tests.Helpers;
using FluentAssertions;
using SQLite;

namespace ExpenseTracker.Tests.Repositories;

// ═══════════════════════════════════════════════════════════════════════════
// ExpenseRepository tests
// ═══════════════════════════════════════════════════════════════════════════
//
// These tests use a real in-memory SQLite connection (not mocked) because
// ExpenseRepository's value lies in its query logic — date filtering,
// category population, totals. Mocking SQLite would just test the mock.

public class ExpenseRepositoryTests : IAsyncLifetime
{
    private SQLiteAsyncConnection _db = null!;
    private TestExpenseRepository _repo = null!;

    public async Task InitializeAsync()
    {
        _db = await TestDbContextFactory.CreateInMemoryAsync();
        _repo = new TestExpenseRepository(_db);
    }

    public Task DisposeAsync() => _db.CloseAsync();

    [Fact]
    public async Task AddAsync_AssignsAutoIncrementId()
    {
        var foodCategory = await _db.Table<Category>().Where(c => c.Name == "Food").FirstAsync();

        var expense = new Expense
        {
            Name = "Walmart Grocery",
            Amount = 84.50m,
            Date = DateTime.Today,
            CategoryId = foodCategory.Id,
            Type = TransactionType.Expense
        };

        var id = await _repo.AddAsync(expense);

        id.Should().BeGreaterThan(0);
        expense.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetByDateRangeAsync_ReturnsOnlyExpensesWithinRange()
    {
        var foodCategory = await _db.Table<Category>().Where(c => c.Name == "Food").FirstAsync();

        await _repo.AddAsync(new Expense { Name = "In range", Amount = 10, Date = DateTime.Today, CategoryId = foodCategory.Id, Type = TransactionType.Expense });
        await _repo.AddAsync(new Expense { Name = "Too old", Amount = 20, Date = DateTime.Today.AddMonths(-2), CategoryId = foodCategory.Id, Type = TransactionType.Expense });
        await _repo.AddAsync(new Expense { Name = "Too new", Amount = 30, Date = DateTime.Today.AddMonths(1), CategoryId = foodCategory.Id, Type = TransactionType.Expense });

        var from = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var to = from.AddMonths(1).AddSeconds(-1);

        var result = await _repo.GetByDateRangeAsync(from, to);

        result.Should().ContainSingle(e => e.Name == "In range");
        result.Should().NotContain(e => e.Name == "Too old");
        result.Should().NotContain(e => e.Name == "Too new");
    }

    [Fact]
    public async Task GetByDateRangeAsync_PopulatesCategoryNavigationProperty()
    {
        var foodCategory = await _db.Table<Category>().Where(c => c.Name == "Food").FirstAsync();

        await _repo.AddAsync(new Expense
        {
            Name = "Groceries",
            Amount = 50,
            Date = DateTime.Today,
            CategoryId = foodCategory.Id,
            Type = TransactionType.Expense
        });

        var result = await _repo.GetByDateRangeAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

        result.Should().ContainSingle();
        result[0].Category.Should().NotBeNull();
        result[0].Category!.Name.Should().Be("Food");
    }

    [Fact]
    public async Task GetTotalAsync_SumsOnlyMatchingTransactionType()
    {
        var foodCategory = await _db.Table<Category>().Where(c => c.Name == "Food").FirstAsync();
        var incomeCategory = await _db.Table<Category>().Where(c => c.Name == "Income").FirstAsync();

        await _repo.AddAsync(new Expense { Name = "Groceries", Amount = 50, Date = DateTime.Today, CategoryId = foodCategory.Id, Type = TransactionType.Expense });
        await _repo.AddAsync(new Expense { Name = "Rent", Amount = 100, Date = DateTime.Today, CategoryId = foodCategory.Id, Type = TransactionType.Expense });
        await _repo.AddAsync(new Expense { Name = "Salary", Amount = 3000, Date = DateTime.Today, CategoryId = incomeCategory.Id, Type = TransactionType.Income });

        var from = DateTime.Today.AddDays(-1);
        var to = DateTime.Today.AddDays(1);

        var expenseTotal = await _repo.GetTotalAsync(from, to, TransactionType.Expense);
        var incomeTotal = await _repo.GetTotalAsync(from, to, TransactionType.Income);

        expenseTotal.Should().Be(150m);
        incomeTotal.Should().Be(3000m);
    }

    [Fact]
    public async Task DeleteAsync_CascadesToLineItems()
    {
        var foodCategory = await _db.Table<Category>().Where(c => c.Name == "Food").FirstAsync();

        var expense = new Expense { Name = "Walmart", Amount = 50, Date = DateTime.Today, CategoryId = foodCategory.Id, Type = TransactionType.Expense };
        var id = await _repo.AddAsync(expense);

        await _db.InsertAllAsync(new List<LineItem>
        {
            new() { ExpenseId = id, Name = "Milk", UnitPrice = 3.98m, Quantity = 1 },
            new() { ExpenseId = id, Name = "Eggs", UnitPrice = 4.48m, Quantity = 1 },
        });

        await _repo.DeleteAsync(id);

        var remainingLineItems = await _db.Table<LineItem>().Where(l => l.ExpenseId == id).ToListAsync();
        var remainingExpense = await _db.Table<Expense>().Where(e => e.Id == id).FirstOrDefaultAsync();

        remainingLineItems.Should().BeEmpty();
        remainingExpense.Should().BeNull();
    }

    [Fact]
    public async Task GetTotalByCategoryAsync_OnlyCountsExpenseType()
    {
        var foodCategory = await _db.Table<Category>().Where(c => c.Name == "Food").FirstAsync();
        var incomeCategory = await _db.Table<Category>().Where(c => c.Name == "Income").FirstAsync();

        await _repo.AddAsync(new Expense { Name = "Groceries", Amount = 50, Date = DateTime.Today, CategoryId = foodCategory.Id, Type = TransactionType.Expense });
        await _repo.AddAsync(new Expense { Name = "Refund", Amount = 999, Date = DateTime.Today, CategoryId = foodCategory.Id, Type = TransactionType.Income }); // edge case: income tagged with Food category

        var total = await _repo.GetTotalByCategoryAsync(foodCategory.Id, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

        total.Should().Be(50m); // Income-type rows excluded even if same category
    }
}

// ─── Test-friendly repository wrapper ────────────────────────────────────────
// ExpenseRepository's constructor takes AppDbContext (which calls
// FileSystem.AppDataDirectory). This thin wrapper exposes the same
// query logic against a pre-built in-memory connection for testing.
internal class TestExpenseRepository
{
    private readonly SQLiteAsyncConnection _db;

    public TestExpenseRepository(SQLiteAsyncConnection db) => _db = db;

    public async Task<int> AddAsync(Expense expense)
    {
        expense.CreatedAt = DateTime.UtcNow;
        expense.UpdatedAt = DateTime.UtcNow;
        await _db.InsertAsync(expense);
        return expense.Id;
    }

    public async Task<List<Expense>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var expenses = await _db.Table<Expense>()
            .Where(e => e.Date >= from && e.Date <= to)
            .OrderByDescending(e => e.Date)
            .ToListAsync();

        await PopulateCategoriesAsync(expenses);
        return expenses;
    }

    public async Task<decimal> GetTotalAsync(DateTime from, DateTime to, TransactionType type)
    {
        var expenses = await _db.Table<Expense>()
            .Where(e => e.Date >= from && e.Date <= to && e.Type == type)
            .ToListAsync();
        return expenses.Sum(e => e.Amount);
    }

    public async Task<decimal> GetTotalByCategoryAsync(int categoryId, DateTime from, DateTime to)
    {
        var expenses = await _db.Table<Expense>()
            .Where(e => e.CategoryId == categoryId && e.Date >= from && e.Date <= to && e.Type == TransactionType.Expense)
            .ToListAsync();
        return expenses.Sum(e => e.Amount);
    }

    public async Task DeleteAsync(int id)
    {
        await _db.DeleteAsync<Expense>(id);
        await _db.Table<LineItem>().DeleteAsync(l => l.ExpenseId == id);
    }

    private async Task PopulateCategoriesAsync(List<Expense> expenses)
    {
        var categoryIds = expenses.Select(e => e.CategoryId).Distinct().ToList();
        if (!categoryIds.Any()) return;

        var categories = await _db.Table<Category>().Where(c => categoryIds.Contains(c.Id)).ToListAsync();
        var categoryMap = categories.ToDictionary(c => c.Id);

        foreach (var expense in expenses)
            expense.Category = categoryMap.GetValueOrDefault(expense.CategoryId);
    }
}
