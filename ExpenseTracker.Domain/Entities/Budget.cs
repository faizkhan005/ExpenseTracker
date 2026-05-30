using SQLite;

namespace ExpenseTracker.Domain.Entities;

/// <summary>
/// Monthly spending limit, optionally scoped to a category.
/// A null CategoryId means it's the overall monthly budget.
/// </summary>
[Table("Budgets")]
public class Budget : BaseEntity
{
    public int Year { get; set; }
    public int Month { get; set; } // 1–12

    [NotNull]
    public decimal LimitAmount { get; set; }

    /// <summary>Null = overall budget. Set = per-category budget.</summary>
    public int? CategoryId { get; set; }

    [Ignore] public Category? Category { get; set; }
}
