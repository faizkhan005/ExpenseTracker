using ExpenseTracker.Application.DTO;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Services;

public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _budgetRepo;
    private readonly IExpenseService _expenseService;
    private readonly ICategoryRepository _categoryRepo;

    public BudgetService(IBudgetRepository budgetRepo, IExpenseService expenseService, ICategoryRepository categoryRepo)
    {
        _budgetRepo = budgetRepo;
        _expenseService = expenseService;
        _categoryRepo = categoryRepo;
    }

    public async Task<decimal> GetMonthlyBudgetAsync(int year, int month)
    {
        var budget = await _budgetRepo.GetAsync(year, month);
        return budget?.LimitAmount ?? 0;
    }

    public async Task SetMonthlyBudgetAsync(int year, int month, decimal amount)
    {
        await _budgetRepo.AddOrUpdateAsync(new Budget
        {
            Year = year,
            Month = month,
            LimitAmount = amount,
            CategoryId = null
        });
    }

    public async Task<decimal> GetCategoryBudgetAsync(int year, int month, int categoryId)
    {
        var budget = await _budgetRepo.GetAsync(year, month, categoryId);
        return budget?.LimitAmount ?? 0;
    }

    public async Task SetCategoryBudgetAsync(int year, int month, int categoryId, decimal amount)
    {
        await _budgetRepo.AddOrUpdateAsync(new Budget
        {
            Year = year,
            Month = month,
            LimitAmount = amount,
            CategoryId = categoryId
        });
    }

    public async Task<BudgetSummary> GetBudgetSummaryAsync(int year, int month)
    {
        var totalBudget = await GetMonthlyBudgetAsync(year, month);
        var totalSpent = await _expenseService.GetTotalSpentAsync(year, month);
        var byCategory = await _expenseService.GetSpendingByCategoryAsync(year, month);
        var allBudgets = await _budgetRepo.GetAllForMonthAsync(year, month);
        var categories = await _categoryRepo.GetAllAsync();

        var categoryLines = categories.Select(cat => new CategoryBudgetLine
        {
            Category = cat,
            Budget = allBudgets.FirstOrDefault(b => b.CategoryId == cat.Id)?.LimitAmount ?? 0,
            Spent = byCategory.GetValueOrDefault(cat.Name, 0)
        })
        .Where(l => l.Spent > 0 || l.Budget > 0)
        .OrderByDescending(l => l.Spent)
        .ToList();

        return new BudgetSummary
        {
            TotalBudget = totalBudget,
            TotalSpent = totalSpent,
            CategoryLines = categoryLines
        };
    }
}
