using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Infrastructure.Data.Repositories;

public class SmsRuleRepository : ISmsRuleRepository
{
    private readonly AppDbContext _context;

    public SmsRuleRepository(AppDbContext context) => _context = context;

    public async Task<List<SmsRule>> GetAllActiveAsync()
    {
        var db = await _context.GetConnectionAsync();
        return await db.Table<SmsRule>().Where(r => r.IsActive).ToListAsync();
    }

    public async Task<int> AddAsync(SmsRule rule)
    {
        var db = await _context.GetConnectionAsync();
        rule.CreatedAt = DateTime.UtcNow;
        rule.UpdatedAt = DateTime.UtcNow;
        await db.InsertAsync(rule);
        return rule.Id;
    }

    public async Task UpdateAsync(SmsRule rule)
    {
        var db = await _context.GetConnectionAsync();
        rule.UpdatedAt = DateTime.UtcNow;
        await db.UpdateAsync(rule);
    }

    public async Task DeleteAsync(int id)
    {
        var db = await _context.GetConnectionAsync();
        await db.DeleteAsync<SmsRule>(id);
    }
}
