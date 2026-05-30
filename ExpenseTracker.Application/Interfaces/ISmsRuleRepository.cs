using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces;

public interface ISmsRuleRepository
{
    Task<List<SmsRule>> GetAllActiveAsync();
    Task<int> AddAsync(SmsRule rule);
    Task UpdateAsync(SmsRule rule);
    Task DeleteAsync(int id);
}
