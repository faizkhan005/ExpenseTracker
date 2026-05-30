using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo) => _repo = repo;

    public Task<List<Category>> GetAllAsync() => _repo.GetAllAsync();
    public Task<int> AddAsync(Category category) => _repo.AddAsync(category);
    public Task UpdateAsync(Category category) => _repo.UpdateAsync(category);
    public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
}
