namespace ExpenseTracker.Models;

public class CategoryIconMap
{
    private static readonly Dictionary<string, string> Glyphs = new()
    {
        ["cart"] = "\ue8cc",
        ["car"] = "\ue531",
        ["home"] = "\ue88a",
        ["fork"] = "\ue56c",
        ["health"] = "\ue548",
        ["phone"] = "\ue0cd",
        ["bag"] = "\uf290",
        ["bank"] = "\ue227",
        ["dots"] = "\ue5d3",
    };

    public static string GetGlyph(string iconKey) =>
        Glyphs.GetValueOrDefault(iconKey, "\ue5d3");
}
