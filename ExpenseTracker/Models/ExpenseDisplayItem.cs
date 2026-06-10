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

        (IconGlyph, IconBackground, IconColor, CategoryBackground, CategoryColor) = e.Category?.Name switch
        {
            "Food" => ("\ue2e7", Color.FromArgb("#EAF3DE"), Color.FromArgb("#3B6D11"), Color.FromArgb("#EAF3DE"), Color.FromArgb("#3B6D11")),
            "Transport" => ("\ue531", Color.FromArgb("#E6F1FB"), Color.FromArgb("#185FA5"), Color.FromArgb("#E6F1FB"), Color.FromArgb("#185FA5")),
            "Housing" => ("\ue88a", Color.FromArgb("#FAECE7"), Color.FromArgb("#712B13"), Color.FromArgb("#FAECE7"), Color.FromArgb("#712B13")),
            "Dining" => ("\ue56c", Color.FromArgb("#FAEEDA"), Color.FromArgb("#633806"), Color.FromArgb("#FAEEDA"), Color.FromArgb("#633806")),
            "Health" => ("\ue548", Color.FromArgb("#E1F5EE"), Color.FromArgb("#0F6E56"), Color.FromArgb("#E1F5EE"), Color.FromArgb("#0F6E56")),
            "Subscriptions" => ("\ue325", Color.FromArgb("#EEEDFE"), Color.FromArgb("#534AB7"), Color.FromArgb("#EEEDFE"), Color.FromArgb("#534AB7")),
            "Shopping" => ("\ue8cc", Color.FromArgb("#FBEAF0"), Color.FromArgb("#72243E"), Color.FromArgb("#FBEAF0"), Color.FromArgb("#72243E")),
            "Income" => ("\ue227", Color.FromArgb("#E1F5EE"), Color.FromArgb("#0F6E56"), Color.FromArgb("#E1F5EE"), Color.FromArgb("#0F6E56")),
            _ => ("\ue5d3", Color.FromArgb("#F1EFE8"), Color.FromArgb("#5F5E5A"), Color.FromArgb("#F1EFE8"), Color.FromArgb("#5F5E5A")),
        };
    }
}
