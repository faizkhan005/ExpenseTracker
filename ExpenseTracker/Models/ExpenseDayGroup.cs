namespace ExpenseTracker.Models;

public partial class ExpenseDayGroup : List<ExpenseDisplayItem>
{
    public string DateLabel { get; }
    public decimal DayTotal { get; }

    public ExpenseDayGroup(string dateLabel, decimal dayTotal, List<ExpenseDisplayItem> items)
        : base(items)
    {
        DateLabel = dateLabel;
        DayTotal = dayTotal;
    }
}
