using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces;

public interface ISmsParserService
{
    /// <summary>Attempts to parse a raw SMS body into an Expense using stored SmsRules.</summary>
    Task<Expense?> TryParseAsync(string smsBody, string sender);
}
