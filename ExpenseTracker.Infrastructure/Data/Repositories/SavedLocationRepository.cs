using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Infrastructure.Data.Repositories;

public class SavedLocationRepository : ISavedLocationRepository
{
    private readonly AppDbContext _context;

    public SavedLocationRepository(AppDbContext context) => _context = context;

    public async Task<List<SavedLocation>> GetAllActiveAsync()
    {
        var db = await _context.GetConnectionAsync();
        return await db.Table<SavedLocation>().Where(l => l.IsActive).ToListAsync();
    }

    public async Task<int> AddAsync(SavedLocation location)
    {
        var db = await _context.GetConnectionAsync();
        location.CreatedAt = DateTime.UtcNow;
        location.UpdatedAt = DateTime.UtcNow;
        await db.InsertAsync(location);
        return location.Id;
    }

    public async Task UpdateAsync(SavedLocation location)
    {
        var db = await _context.GetConnectionAsync();
        location.UpdatedAt = DateTime.UtcNow;
        await db.UpdateAsync(location);
    }

    public async Task DeleteAsync(int id)
    {
        var db = await _context.GetConnectionAsync();
        await db.DeleteAsync<SavedLocation>(id);
    }
}
