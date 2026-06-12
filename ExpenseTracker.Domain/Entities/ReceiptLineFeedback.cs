using SQLite;

namespace ExpenseTracker.Domain.Entities;

public class ReceiptLineFeedback
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull, MaxLength(300)]
    public string Text { get; set; } = string.Empty;

    [NotNull, MaxLength(20)]
    public string CorrectedLabel { get; set; } = string.Empty; // Product/Total/Tax/Subtotal/Merchant/Noise

    public float PositionRatio { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
