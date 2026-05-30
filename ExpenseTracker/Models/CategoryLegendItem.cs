namespace ExpenseTracker.Models
{
    public class CategoryLegendItem
    {
        public string Name { get; set; } = string.Empty;
        public int Percentage { get; set; }
        public Color Color { get; set; } = Colors.Gray;
    }
}
