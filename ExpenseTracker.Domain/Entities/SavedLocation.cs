using SQLite;

namespace ExpenseTracker.Domain.Entities;

/// <summary>
/// A geofenced location that triggers a purchase prompt when the user leaves.
/// e.g. Walmart at lat/lng with 100m radius.
/// </summary>
[Table("SavedLocations")]
public class SavedLocation : BaseEntity
{
    [NotNull, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    /// <summary>Geofence radius in metres.</summary>
    public double RadiusMetres { get; set; } = 100;

    public int DefaultCategoryId { get; set; }

    public bool IsActive { get; set; } = true;
}
