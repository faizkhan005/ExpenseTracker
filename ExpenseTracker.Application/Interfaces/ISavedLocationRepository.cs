using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces;

public interface ISavedLocationRepository
{
    Task<List<SavedLocation>> GetAllActiveAsync();
    Task<int> AddAsync(SavedLocation location);
    Task UpdateAsync(SavedLocation location);
    Task DeleteAsync(int id);
}
