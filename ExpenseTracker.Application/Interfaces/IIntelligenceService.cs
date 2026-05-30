using ExpenseTracker.Application.DTO;

namespace ExpenseTracker.Application.Interfaces;

public interface IIntelligenceService
{
    /// <summary>Rule-based savings suggestions for the current month.</summary>
    Task<List<SavingsTip>> GetSavingsTipsAsync(int year, int month);

    /// <summary>Predicts next month's spend using weighted moving average.</summary>
    Task<decimal> PredictNextMonthSpendAsync();

    /// <summary>Recommends grocery quantities based on 3-month purchase history.</summary>
    Task<List<QuantityRecommendation>> GetQuantityRecommendationsAsync();

    /// <summary>Calls Claude API for smart contextual tips.</summary>
    Task<List<SavingsTip>> GetAiSavingsTipsAsync(int year, int month);
}
