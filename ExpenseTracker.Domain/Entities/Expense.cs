using ExpenseTracker.Domain.Enums;
using SQLite;

namespace ExpenseTracker.Domain.Entities;

/// <summary>
/// Core transaction record. Covers both expenses and income.
/// </summary>
[Table("Expenses")]
public class Expense : BaseEntity
{
    [NotNull, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [NotNull]
    public decimal Amount { get; set; }

    [NotNull]
    public DateTime Date { get; set; }

    /// <summary>FK → Categories.Id</summary>
    public int CategoryId { get; set; }

    public TransactionType Type { get; set; } = TransactionType.Expense;

    public ExpenseSource Source { get; set; } = ExpenseSource.Manual;

    /// <summary>Optional notes or merchant details.</summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>Path to scanned receipt image stored locally.</summary>
    [MaxLength(500)]
    public string? ReceiptImagePath { get; set; }

    /// <summary>FK → RecurringExpenses.Id — set if auto-logged from a recurring rule.
    /// </summary>
    public int? RecurringExpenseId { get; set; }

    // ── Navigation properties (not stored in SQLite, populated manually) ──
    [Ignore]
    public Category? Category { get; set; }
    [Ignore]
    public List<LineItem> LineItems { get; set; } = new();
}
