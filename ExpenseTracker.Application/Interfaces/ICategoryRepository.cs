using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<Category?> GetByNameAsync(string name);
    Task<int> AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(int id); // Only non-system categories
}
