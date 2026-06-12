using Microsoft.ML.Data;

namespace ExpenseTracker.Application.DTO;

public class ReceiptLinePrediction
{
    [ColumnName("PredictedLabel")]
    public string PredictedLabel { get; set; } = string.Empty;

    public float[] Score { get; set; } = [];
}
