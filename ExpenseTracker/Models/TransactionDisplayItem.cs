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
        IconName = expense.Category?.IconGlyph ?? "\ue5d3";
        IconBackground = Color.FromArgb(expense.Category?.BackgroundHex ?? "#F1EFE8");
        IconColor = Color.FromArgb(expense.Category?.ColorHex ?? "#5F5E5A");
    }
}
