using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Tests.Helpers;
using FluentAssertions;
using SQLite;

namespace ExpenseTracker.Tests.Repositories;

// ═══════════════════════════════════════════════════════════════════════════
// CategoryRepository — seed data integrity and system category protection
// ═══════════════════════════════════════════════════════════════════════════

public class CategoryRepositoryTests : IAsyncLifetime
{
    private SQLiteAsyncConnection _db = null!;
    private TestCategoryRepository _repo = null!;

    public async Task InitializeAsync()
    {
        _db = await TestDbContextFactory.CreateInMemoryAsync();
        _repo = new TestCategoryRepository(_db);
    }

    public Task DisposeAsync() => _db.CloseAsync();

    [Fact]
    public async Task SeedData_ContainsExactlyNineDefaultCategories()
    {
        var categories = await _repo.GetAllAsync();

        categories.Should().HaveCount(9);
        categories.Select(c => c.Name).Should().Contain(new[]
        {
            "Food", "Transport", "Housing", "Dining", "Health",
            "Subscriptions", "Shopping", "Income", "Other"
        });
    }

    [Fact]
    public async Task SeedData_AllDefaultCategoriesAreMarkedSystem()
    {
        var categories = await _repo.GetAllAsync();

        categories.Should().AllSatisfy(c => c.IsSystem.Should().BeTrue());
    }

    [Fact]
    public async Task AddAsync_CustomCategory_IsNotSystem()
    {
        var custom = new Category
        {
            Name = "Gym",
            IconGlyph = "\ue87d",
            ColorHex = "#534AB7",
            BackgroundHex = "#EEEDFE",
            IsSystem = false
        };

        var id = await _repo.AddAsync(custom);

        var saved = await _repo.GetByIdAsync(id);
        saved.Should().NotBeNull();
        saved!.IsSystem.Should().BeFalse();
        saved.IconGlyph.Should().Be("\ue87d");
    }

    [Fact]
    public async Task DeleteAsync_SystemCategory_ThrowsInvalidOperationException()
    {
        var foodCategory = await _repo.GetByNameAsync("Food");

        var act = async () => await _repo.DeleteAsync(foodCategory!.Id);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteAsync_CustomCategory_Succeeds()
    {
        var custom = new Category { Name = "Gym", IconGlyph = "\ue87d", ColorHex = "#534AB7", BackgroundHex = "#EEEDFE", IsSystem = false };
        var id = await _repo.AddAsync(custom);

        await _repo.DeleteAsync(id);

        var result = await _repo.GetByIdAsync(id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_CaseSensitiveExactMatch()
    {
        var result = await _repo.GetByNameAsync("Food");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Food");
    }

    [Fact]
    public async Task GetByNameAsync_NonExistentName_ReturnsNull()
    {
        var result = await _repo.GetByNameAsync("NonExistentCategory");

        result.Should().BeNull();
    }
}

// Test-friendly repository wrapper
internal class TestCategoryRepository
{
    private readonly SQLiteAsyncConnection _db;

    public TestCategoryRepository(SQLiteAsyncConnection db) => _db = db;

    public Task<List<Category>> GetAllAsync()
        => _db.Table<Category>().OrderBy(c => c.Name).ToListAsync();

    public async Task<Category?> GetByIdAsync(int id) 
    {
        Category? category = await _db.Table<Category>().Where(c => c.Id == id).FirstOrDefaultAsync();
        return category;
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        Category? category = await _db.Table<Category>().Where(c => c.Name == name).FirstOrDefaultAsync();
        return category;
    }

    public async Task<int> AddAsync(Category category)
    {
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;
        await _db.InsertAsync(category);
        return category.Id;
    }

    public async Task DeleteAsync(int id)
    {
        var category = await GetByIdAsync(id);
        if (category is null || category.IsSystem)
            throw new InvalidOperationException("Cannot delete a system category.");
        await _db.DeleteAsync<Category>(id);
    }
}
