using ExpenseTracker.Domain.Enums;
using SQLite;

namespace ExpenseTracker.Domain.Entities;

/// <summary>
/// A rule that auto-creates an Expense on a schedule.
/// e.g. Rent $1200 on the 1st of every month.
/// </summary>
[Table("RecurringExpenses")]
public class RecurringExpense : BaseEntity
{
    [NotNull, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [NotNull]
    public decimal Amount { get; set; }

    public int CategoryId { get; set; }

    public RecurrenceFrequency Frequency { get; set; } = RecurrenceFrequency.Monthly;

    /// <summary>
    /// Day of month (1–31) for Monthly, or day of week (0–6) for Weekly.
    /// </summary>
    public int DayOfPeriod { get; set; } = 1;

    public DateTime StartDate { get; set; } = DateTime.Today;

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Last date this rule successfully created an Expense.</summary>
    public DateTime? LastProcessedDate { get; set; }

    [Ignore] public Category? Category { get; set; }
}
