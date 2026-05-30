namespace ExpenseTracker.Application.DTO;

public class LineItemHistory
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime PurchaseDate { get; set; }
}
