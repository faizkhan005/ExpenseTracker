using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Models;

public class ExpenseDisplayItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AmountFormatted { get; set; } = string.Empty;
    public Color AmountColor { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Color CategoryColor { get; set; }
    public Color CategoryBackground { get; set; }
    public Color IconBackground { get; set; }
    public Color IconColor { get; set; }
    public string IconGlyph { get; set; } = string.Empty;
    public string SourceLabel { get; set; } = string.Empty;
    public string TimeLabel { get; set; } = string.Empty;


    public ExpenseDisplayItem(Expense e)
    {
        Id = e.Id;
        Name = e.Name;
        AmountFormatted = e.Type == TransactionType.Income ? $"+{e.Amount:C}" : $"-{e.Amount:C}";
        AmountColor = e.Type == TransactionType.Income ? Color.FromArgb("#1D9E75") : Color.FromArgb("#E24B4A");
        CategoryName = e.Category?.Name ?? "Other";
        TimeLabel = e.Date.ToString("h:mm tt");

        SourceLabel = e.Source switch
        {
            ExpenseSource.Sms => "via SMS",
            ExpenseSource.Ocr => "receipt scanned",
            ExpenseSource.Recurring => "auto · recurring",
            ExpenseSource.Location => "location prompt",
            _ => "manual"
        };

        IconGlyph = e.Category?.IconGlyph ?? "\ue5d3";
        IconBackground = Color.FromArgb(e.Category?.BackgroundHex ?? "#F1EFE8");
        IconColor = Color.FromArgb(e.Category?.ColorHex ?? "#5F5E5A");
        CategoryBackground = Color.FromArgb(e.Category?.BackgroundHex ?? "#F1EFE8");
        CategoryColor = Color.FromArgb(e.Category?.ColorHex ?? "#5F5E5A");
    }
}
