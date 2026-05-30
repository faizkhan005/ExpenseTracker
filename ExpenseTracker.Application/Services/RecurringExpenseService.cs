using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Services;

public class RecurringExpenseService : IRecurringExpenseService
{
    private readonly IRecurringExpenseRepository _recurringRepo;
    private readonly IExpenseService _expenseService;
    private readonly ICategoryRepository _categoryRepo;

    public RecurringExpenseService(
        IRecurringExpenseRepository recurringRepo,
        IExpenseService expenseService,
        ICategoryRepository categoryRepo)
    {
        _recurringRepo = recurringRepo;
        _expenseService = expenseService;
        _categoryRepo = categoryRepo;
    }

    public Task<List<RecurringExpense>> GetAllAsync()
        => _recurringRepo.GetAllActiveAsync();

    public Task<int> AddAsync(RecurringExpense rule)
        => _recurringRepo.AddAsync(rule);

    public Task UpdateAsync(RecurringExpense rule)
        => _recurringRepo.UpdateAsync(rule);

    public Task DeleteAsync(int id)
        => _recurringRepo.DeleteAsync(id);

    public async Task ProcessDueRecurringExpensesAsync()
    {
        var dueRules = await _recurringRepo.GetDueTodayAsync();

        foreach (var rule in dueRules)
        {
            var expense = new Expense
            {
                Name = rule.Name,
                Amount = rule.Amount,
                Date = DateTime.Today,
                CategoryId = rule.CategoryId,
                Type = TransactionType.Expense,
                Source = ExpenseSource.Recurring,
                RecurringExpenseId = rule.Id,
                Notes = $"Auto-logged from recurring rule: {rule.Name}"
            };

            await _expenseService.AddExpenseAsync(expense);

            // Update last processed date so it doesn't fire again today
            rule.LastProcessedDate = DateTime.Today;
            await _recurringRepo.UpdateAsync(rule);
        }
    }
}
