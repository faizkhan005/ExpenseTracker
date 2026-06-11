using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Models;

public class TransactionDisplayItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AmountFormatted { get; set; } = string.Empty;
    public Color AmountColor { get; set; }
    public string CategoryAndTime { get; set; } = string.Empty;
    public string IconName { get; set; } = string.Empty;
    public Color IconBackground { get; set; }
    public Color IconColor { get; set; }

    // Construct directly from domain Expense entity
    public TransactionDisplayItem(Expense expense)
    {
        Id = expense.Id;
        Name = expense.Name;

        // Use TransactionType enum — NOT a boolean IsIncome
        AmountFormatted = expense.Type == TransactionType.Income
            ? $"+{expense.Amount:C}"
            : $"-{expense.Amount:C}";

        AmountColor = expense.Type == TransactionType.Income
            ? Color.FromArgb("#1D9E75")
            : Color.FromArgb("#E24B4A");

        var timeLabel = expense.Date.Date == DateTime.Today
            ? $"Today {expense.Date:h:mm tt}"
            : expense.Date.Date == DateTime.Today.AddDays(-1)
                ? "Yesterday"
                : expense.Date.ToString("MMM d");

        CategoryAndTime = $"{expense.Category?.Name ?? "Other"} · {timeLabel}";

        // Map Category.Name to icon asset name and colors
        (IconName, IconBackground, IconColor) = expense.Category?.Name switch
        {
            "Food" => ("\ue2e7", Color.FromArgb("#EAF3DE"), Color.FromArgb("#3B6D11")),
            "Transport" => ("\ue531", Color.FromArgb("#E6F1FB"), Color.FromArgb("#185FA5")),
            "Housing" => ("\ue88a", Color.FromArgb("#FAECE7"), Color.FromArgb("#712B13")),
            "Dining" => ("\ue56c", Color.FromArgb("#FAEEDA"), Color.FromArgb("#633806")),
            "Health" => ("\ue548", Color.FromArgb("#E1F5EE"), Color.FromArgb("#0F6E56")),
            "Subscriptions" => ("\ue325", Color.FromArgb("#EEEDFE"), Color.FromArgb("#534AB7")),
            "Shopping" => ("\ue8cc", Color.FromArgb("#FBEAF0"), Color.FromArgb("#72243E")),
            "Income" => ("\ue227", Color.FromArgb("#E1F5EE"), Color.FromArgb("#0F6E56")),
            _ => ("\ue5d3", Color.FromArgb("#F1EFE8"), Color.FromArgb("#5F5E5A")),
        };
    }
}
