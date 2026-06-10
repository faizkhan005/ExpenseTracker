using ExpenseTracker.Application.DTO;

namespace ExpenseTracker.Models;

public class SavingsTipDisplay
{
    public string Title { get; set; }
    public string Body { get; set; }
    public decimal PotentialSaving { get; set; }
    public string CategoryName { get; set; }
    public string CategoryIconGlyph { get; set; }
    public Color CategoryBackground { get; set; }
    public Color CategoryIconColor { get; set; }

    public SavingsTipDisplay(SavingsTip tip)
    {
        Title = tip.Title;
        Body = tip.Body;
        PotentialSaving = tip.PotentialSaving;
        CategoryName = tip.CategoryName;

        (CategoryIconGlyph, CategoryBackground, CategoryIconColor) = tip.CategoryName switch
        {
            "Food" => ("\ue2e7", Color.FromArgb("#EAF3DE"), Color.FromArgb("#3B6D11")),
            "Dining" => ("\ue56c", Color.FromArgb("#FAEEDA"), Color.FromArgb("#633806")),
            "Transport" => ("\ue531", Color.FromArgb("#E6F1FB"), Color.FromArgb("#185FA5")),
            "Subscriptions" => ("\ue325", Color.FromArgb("#EEEDFE"), Color.FromArgb("#534AB7")),
            _ => ("\ue8dc", Color.FromArgb("#E1F5EE"), Color.FromArgb("#0F6E56")),
        };
    }
}
