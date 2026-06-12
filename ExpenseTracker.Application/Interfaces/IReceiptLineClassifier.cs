using ExpenseTracker.Application.DTO;

namespace ExpenseTracker.Application.Interfaces;

public interface IReceiptLineClassifier
{
    OcrResult ParseReceiptLines(List<string> lines);
}
