using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.DTO;

public class OcrResult
{
    public List<LineItem> LineItems { get; set; } = new();
    public decimal Total { get; set; }
    public string? MerchantName { get; set; }
    public DateTime? ReceiptDate { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
}
