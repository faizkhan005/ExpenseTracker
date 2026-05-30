using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Infrastructure.Data.Repositories;

public class RecurringExpenseRepository : IRecurringExpenseRepository   
{
    private readonly AppDbContext _context;

    public RecurringExpenseRepository(AppDbContext context) => _context = context;

    public async Task<List<RecurringExpense>> GetAllActiveAsync()
    {
        var db = await _context.GetConnectionAsync();
        return await db.Table<RecurringExpense>().Where(r => r.IsActive).ToListAsync();
    }

    public async Task<RecurringExpense?> GetByIdAsync(int id)
    {
        var db = await _context.GetConnectionAsync();
        return await db.Table<RecurringExpense>().Where(r => r.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<RecurringExpense>> GetDueTodayAsync()
    {
        var db = await _context.GetConnectionAsync();
        var today = DateTime.Today;
        var all = await db.Table<RecurringExpense>().Where(r => r.IsActive).ToListAsync();

        return all.Where(r => IsDueToday(r, today)).ToList();
    }

    public async Task<int> AddAsync(RecurringExpense rule)
    {
        var db = await _context.GetConnectionAsync();
        rule.CreatedAt = DateTime.UtcNow;
        rule.UpdatedAt = DateTime.UtcNow;
        await db.InsertAsync(rule);
        return rule.Id;
    }

    public async Task UpdateAsync(RecurringExpense rule)
    {
        var db = await _context.GetConnectionAsync();
        rule.UpdatedAt = DateTime.UtcNow;
        await db.UpdateAsync(rule);
    }
    public async Task DeleteAsync(int id)
    {
        var db = await _context.GetConnectionAsync();
        await db.DeleteAsync<RecurringExpense>(id);
    }

    private static bool IsDueToday(RecurringExpense rule, DateTime today)
    {
        if (rule.StartDate > today) return false;
        if (rule.EndDate.HasValue && rule.EndDate < today) return false;

        // Don't fire twice on the same day
        if (rule.LastProcessedDate?.Date == today) return false;

        return rule.Frequency switch
        {
            RecurrenceFrequency.Daily => true,
            RecurrenceFrequency.Weekly => (int)today.DayOfWeek == rule.DayOfPeriod,
            RecurrenceFrequency.Monthly => today.Day == rule.DayOfPeriod,
            RecurrenceFrequency.Yearly => today.Day == rule.DayOfPeriod && today.Month == rule.StartDate.Month,
            _ => false
        };
    }
}
