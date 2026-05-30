namespace ExpenseTracker.Application.DTO;

public class SavingsTip
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public decimal PotentialSaving { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsAiGenerated { get; set; }
}
