namespace ExpenseTracker.Models;

public class NotificationItem
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string IconGlyph { get; set; } = "\ue7f4";
    public Color IconColor { get; set; } = Color.FromArgb("#534AB7");
    public Color IconBackground { get; set; } = Color.FromArgb("#EEEDFE");
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsRead { get; set; } = false;
    public string TimeLabel => CreatedAt.Date == DateTime.Today
        ? CreatedAt.ToString("h:mm tt")
        : CreatedAt.ToString("MMM d");
}
