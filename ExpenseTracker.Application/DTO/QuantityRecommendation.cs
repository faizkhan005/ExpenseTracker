namespace ExpenseTracker.Application.DTO;

public class QuantityRecommendation
{
    public string ProductName { get; set; } = string.Empty;
    public double AverageMonthlyQty { get; set; }
    public int RecommendedQty { get; set; }
    public string Unit { get; set; } = string.Empty;
}
