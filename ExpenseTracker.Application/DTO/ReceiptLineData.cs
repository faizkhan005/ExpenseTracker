using Microsoft.ML.Data;

namespace ExpenseTracker.Application.DTO;

public class ReceiptLineData
{
    [LoadColumn(0)] public string Text { get; set; } = string.Empty;
    [LoadColumn(1)] public string Label { get; set; } = string.Empty; // Product/Total/Tax/etc

    // Engineered features — computed from Text at training and inference time
    [LoadColumn(2)] public float HasPrice { get; set; }
    [LoadColumn(3)] public float WordCount { get; set; }
    [LoadColumn(4)] public float HasQuantity { get; set; }
    [LoadColumn(5)] public float PositionRatio { get; set; } // line position / total lines
    [LoadColumn(6)] public float IsAllCaps { get; set; }
    [LoadColumn(7)] public float HasDollarSign { get; set; }
}
