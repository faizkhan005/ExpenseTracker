using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces;

public interface IFeedbackRepository
{
    Task LogCorrectionAsync(string text, string correctedLabel, float positionRatio);
    Task<List<ReceiptLineFeedback>> GetAllAsync();
    Task<int> GetCountAsync();
    Task<string> ExportAsCsvAsync();
}
