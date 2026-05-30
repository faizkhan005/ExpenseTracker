using SQLite;

namespace ExpenseTracker.Domain.Entities;

/// <summary>
/// Individual items extracted from a scanned receipt (OCR).
/// e.g. "Whole milk 1gal x2 = $7.96"
/// </summary>
[Table("LineItems")]
public class LineItem : BaseEntity
{
    /// <summary>FK → Expenses.Id</summary>
    [NotNull]
    public int ExpenseId { get; set; }

    [NotNull, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; } = 1;

    public decimal TotalPrice => UnitPrice * Quantity;

    /// <summary>Raw text extracted by OCR before parsing.</summary>
    [MaxLength(300)]
    public string? RawOcrText { get; set; }
}
