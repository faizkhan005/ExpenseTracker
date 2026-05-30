using ExpenseTracker.Application.DTO;

namespace ExpenseTracker.Application.Interfaces;

public interface IOcrService
{
    /// <summary>Extracts line items and total from a receipt image.</summary>
    Task<OcrResult> ScanReceiptAsync(Stream imageStream);
}
