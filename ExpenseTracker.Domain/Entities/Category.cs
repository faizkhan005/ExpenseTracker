using SQLite;

namespace ExpenseTracker.Domain.Entities;

[Table("Categories")]
/// <summary>
/// Expense category e.g. Food, Transport, Housing.
/// Seeded with defaults on first run; user can add custom ones.
/// </summary>
public class Category : BaseEntity
{
    [NotNull, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Stores the actual Material Icons glyph e.g. "\ue2e7
    /// </summary>
    [MaxLength(50)]
    public string IconGlyph { get; set; } = string.Empty;

    /// <summary>Hex color for the icon foreground e.g. #3B6D11
    /// </summary>
    [MaxLength(9)]
    public string ColorHex { get; set; } = "#534AB7";

    /// <summary>Hex color for the icon background chip e.g. #EAF3DE</summary>
    [MaxLength(9)]
    public string BackgroundHex { get; set; } = "#EEEDFE";

    /// <summary>System categories cannot be deleted by the user.
    /// </summary>
    public bool IsSystem { get; set; } = false;
}
