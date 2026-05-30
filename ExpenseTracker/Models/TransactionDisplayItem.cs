namespace ExpenseTracker.Models
{
    public class TransactionDisplayItem
    {
        public string Name { get; set; } = string.Empty;
        public string CategoryAndTime { get; set; } = string.Empty;
        public string AmountFormatted { get; set; } = string.Empty;
        public Color AmountColor { get; set; } = Colors.Black;
        public string IconName { get; set; } = string.Empty;
        public Color IconBackground { get; set; } = Colors.LightGray;
        public Color IconColor { get; set; } = Colors.Gray;

        // Parameterless (design-time / manual construction)
        public TransactionDisplayItem() { }

        // From domain entity
        public TransactionDisplayItem(Expense expense)
        {
            Name = expense.Name;
            AmountFormatted = expense.IsIncome
                ? $"+{expense.Amount:C}"
                : $"-{expense.Amount:C}";
            AmountColor = expense.IsIncome
                ? Color.FromArgb("#1D9E75")
                : Color.FromArgb("#E24B4A");

            var timeLabel = expense.Date.Date == DateTime.Today
                ? $"Today {expense.Date:h:mm tt}"
                : expense.Date.Date == DateTime.Today.AddDays(-1)
                    ? "Yesterday"
                    : expense.Date.ToString("MMM d");

            CategoryAndTime = $"{expense.Category?.Name ?? "Other"} · {timeLabel}";

            // Map category to icon/colors
            (IconName, IconBackground, IconColor) = expense.Category?.Name switch
            {
                "Food" => ("icon_cart.png", Color.FromArgb("#EAF3DE"), Color.FromArgb("#3B6D11")),
                "Transport" => ("icon_car.png", Color.FromArgb("#E6F1FB"), Color.FromArgb("#185FA5")),
                "Dining" => ("icon_fork.png", Color.FromArgb("#FAEEDA"), Color.FromArgb("#633806")),
                "Housing" => ("icon_home.png", Color.FromArgb("#FAECE7"), Color.FromArgb("#712B13")),
                "Health" => ("icon_health.png", Color.FromArgb("#E1F5EE"), Color.FromArgb("#0F6E56")),
                "Subscriptions" => ("icon_phone.png", Color.FromArgb("#EEEDFE"), Color.FromArgb("#534AB7")),
                "Income" => ("icon_bank.png", Color.FromArgb("#E1F5EE"), Color.FromArgb("#0F6E56")),
                _ => ("icon_other.png", Color.FromArgb("#F1EFE8"), Color.FromArgb("#5F5E5A")),
            };
        }
    }
}
