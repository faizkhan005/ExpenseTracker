using SQLite;

namespace ExpenseTracker.Domain.Entities;

/// <summary>
/// Pattern used to parse bank SMS alerts into Expense records.
/// e.g. "charged $45.50 at Walmart" → Amount=45.50, Name=Walmart
/// Android only.
/// </summary>
[Table("SmsRules")]
public class SmsRule : BaseEntity
{
    [NotNull, MaxLength(100)]
    public string BankName { get; set; } = string.Empty;

    /// <summary>Regex pattern to match the SMS body.</summary>
    [NotNull, MaxLength(500)]
    public string Pattern { get; set; } = string.Empty;

    /// <summary>Named capture group for amount e.g. (?&lt;amount&gt;[\d.]+)</summary>
    [MaxLength(50)]
    public string AmountGroup { get; set; } = "amount";

    /// <summary>Named capture group for merchant name.</summary>
    [MaxLength(50)]
    public string MerchantGroup { get; set; } = "merchant";

    public int DefaultCategoryId { get; set; }

    public bool IsActive { get; set; } = true;
}
