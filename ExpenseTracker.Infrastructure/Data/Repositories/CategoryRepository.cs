using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Infrastructure.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context) => _context = context;

    public async Task<List<Category>> GetAllAsync()
    {
        var db = await _context.GetConnectionAsync();
        return await db.Table<Category>().OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        var db = await _context.GetConnectionAsync();
        return await db.Table<Category>().Where(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        var db = await _context.GetConnectionAsync();
        return await db.Table<Category>().Where(c => c.Name == name).FirstOrDefaultAsync();
    }

    public async Task<int> AddAsync(Category category)
    {
        var db = await _context.GetConnectionAsync();
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;
        await db.InsertAsync(category);
        return category.Id;
    }

    public async Task UpdateAsync(Category category)
    {
        var db = await _context.GetConnectionAsync();
        category.UpdatedAt = DateTime.UtcNow;
        await db.UpdateAsync(category);
    }

    public async Task DeleteAsync(int id)
    {
        var db = await _context.GetConnectionAsync();
        var category = await db.Table<Category>().Where(c => c.Id == id).FirstOrDefaultAsync();
        if (category is null || category.IsSystem)
            throw new InvalidOperationException("Cannot delete a system category.");
        await db.DeleteAsync<Category>(id);
    }
}
