namespace ExpenseTracker.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public Category? Category { get; set; }
        public bool IsIncome { get; set; }
        public string Source { get; set; } = "manual"; // "manual" | "sms" | "ocr" | "recurring"
    }
}
