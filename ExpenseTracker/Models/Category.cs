namespace ExpenseTracker.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IconName { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#534AB7";
        public string BgColorHex { get; set; } = "#EEEDFE";
    }
}
