using ExpenseTracker.Application.DTO;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Services;

public class IntelligenceService : IIntelligenceService
{
    private readonly IExpenseService _expenseService;
    private readonly ILineItemRepository _lineItemRepo;

    public IntelligenceService(IExpenseService expenseService, ILineItemRepository lineItemRepo)
    {
        _expenseService = expenseService;
        _lineItemRepo = lineItemRepo;
    }

    public async Task<List<SavingsTip>> GetSavingsTipsAsync(int year, int month)
    {
        var tips = new List<SavingsTip>();
        var thisMonth = await _expenseService.GetSpendingByCategoryAsync(year, month);

        // Compare vs previous month
        var prev = DateTime.Today.AddMonths(-1);
        var lastMonth = await _expenseService.GetSpendingByCategoryAsync(prev.Year, prev.Month);

        foreach (var (category, spent) in thisMonth)
        {
            var lastSpent = lastMonth.GetValueOrDefault(category, 0);
            if (lastSpent == 0) continue;

            var changePercent = (double)((spent - lastSpent) / lastSpent * 100);

            if (changePercent > 20)
            {
                tips.Add(new SavingsTip
                {
                    CategoryName = category,
                    Title = $"High {category} spend this month",
                    Body = $"You spent {spent:C} on {category} — {changePercent:0}% above last month ({lastSpent:C}).",
                    PotentialSaving = spent - lastSpent,
                    IsAiGenerated = false
                });
            }
        }

        // Dining-specific rule: flag if dining > 15% of total
        var total = thisMonth.Values.Sum();
        var dining = thisMonth.GetValueOrDefault("Dining", 0);
        if (total > 0 && dining / total > 0.15m)
        {
            tips.Add(new SavingsTip
            {
                CategoryName = "Dining",
                Title = "Dining out is eating your budget",
                Body = $"Dining is {(dining / total * 100):0}% of your monthly spend. Cooking 2 extra meals a week could save you ${dining * 0.3m:0}/month.",
                PotentialSaving = dining * 0.3m,
                IsAiGenerated = false
            });
        }
        return tips.OrderByDescending(t => t.PotentialSaving).ToList();
    }

    public async Task<decimal> PredictNextMonthSpendAsync()
    {
        // Weighted average: most recent month counts double
        var now = DateTime.Today;
        var months = new List<(int year, int month, double weight)>
        {
            (now.Year, now.Month, 3.0),
            (now.AddMonths(-1).Year, now.AddMonths(-1).Month, 2.0),
            (now.AddMonths(-2).Year, now.AddMonths(-2).Month, 1.0),
        };

        double totalWeight = 0;
        double weightedSum = 0;
        foreach (var (y, m, w) in months)
        {
            var spent = await _expenseService.GetTotalSpentAsync(y, m);
            if (spent == 0) continue;
            weightedSum += (double)spent * w;
            totalWeight += w;
        }

        return totalWeight == 0 ? 0 : (decimal)(weightedSum / totalWeight);
    }

    public async Task<List<QuantityRecommendation>> GetQuantityRecommendationsAsync()
    {
        // Get all line items from the last 3 months and group by product name
        var from = DateTime.Today.AddMonths(-3);
        var expenses = await _expenseService.GetExpensesAsync(from, DateTime.Today);
        var expenseIds = expenses.Select(e => e.Id).ToList();

        // Aggregate via history lookup per distinct product
        var allItems = new List<LineItem>();
        foreach (var expense in expenses)
            allItems.AddRange(expense.LineItems);

        return allItems
            .GroupBy(l => l.Name.Trim().ToLower())
            .Select(g =>
            {
                var avgQty = g.Average(l => l.Quantity);
                return new QuantityRecommendation
                {
                    ProductName = g.First().Name,
                    AverageMonthlyQty = Math.Round(avgQty, 1),
                    RecommendedQty = (int)Math.Ceiling(avgQty),
                    Unit = "units"
                };
            })
            .Where(r => r.AverageMonthlyQty > 0)
            .OrderByDescending(r => r.AverageMonthlyQty)
            .Take(20)
            .ToList();
    }

    public async Task<List<SavingsTip>> GetAiSavingsTipsAsync(int year, int month)
    {
        var byCategory = await _expenseService.GetSpendingByCategoryAsync(year, month);
        var totalSpent = await _expenseService.GetTotalSpentAsync(year, month);

        // Build a concise spending summary to send to Claude
        var summary = string.Join("\n", byCategory.Select(kvp =>
            $"- {kvp.Key}: {kvp.Value:C}"));

        var prompt =
            $"Here is my spending for this month (total: {totalSpent:C}):\n{summary}\n\n" +
            $"Give me 3 specific, actionable savings tips. " +
            $"Respond ONLY as a JSON array with objects: " +
            $"[{{\"title\": \"...\", \"body\": \"...\", \"potentialSaving\": 0.0}}]. " +
            $"No preamble, no markdown.";
        try
        {
            using var http = new HttpClient();
            var requestBody = System.Text.Json.JsonSerializer.Serialize(new
            {
                model = "claude-sonnet-4-20250514",
                max_tokens = 500,
                messages = new[] { new { role = "user", content = prompt } }
            });

            // Note: API key is injected via the Anthropic SDK or environment variable
            // Replace with your actual Anthropic HttpClient setup
            var response = await http.PostAsync(
                "https://api.anthropic.com/v1/messages",
                new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                return new List<SavingsTip>();

            var json = await response.Content.ReadAsStringAsync();
            var parsed = System.Text.Json.JsonDocument.Parse(json);
            var content = parsed.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? "[]";
            var tips = System.Text.Json.JsonSerializer.Deserialize<List<AiTipDto>>(content)
                       ?? new List<AiTipDto>();

            return tips.Select(t => new SavingsTip
            {
                Title = t.title,
                Body = t.body,
                PotentialSaving = (decimal)t.potentialSaving,
                IsAiGenerated = true
            }).ToList();
        }
        catch
        {
            // If AI call fails, fall back gracefully — return empty, UI shows rule-based tips
            return [];
        }
    }
    private record AiTipDto(string title, string body, double potentialSaving);
}
