using ExpenseTracker.Application.DTO;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using Microsoft.ML;
using System.Text.RegularExpressions;

namespace ExpenseTracker.Application.Services;

public class MlNetReceiptClassifier : IReceiptLineClassifier
{
    private readonly MLContext _mlContext;
    private readonly PredictionEngine<ReceiptLineData, ReceiptLinePrediction>? _predictionEngine;
    private readonly RuleBasedReceiptClassifier _fallback;

    private const string ModelPath = "receipt_classifier.zip";

    public MlNetReceiptClassifier()
    {
        _mlContext = new MLContext(seed: 42);
        _fallback = new RuleBasedReceiptClassifier();

        var modelFilePath = Path.Combine(FileSystem.AppDataDirectory, ModelPath);

        if (File.Exists(modelFilePath))
        {
            var model = _mlContext.Model.Load(modelFilePath, out _);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<ReceiptLineData, ReceiptLinePrediction>(model);
        }
        // If model doesn't exist yet, _predictionEngine stays null
        // and we fall back to rule-based classification
    }

    public OcrResult ParseReceiptLines(List<string> lines)
    {
        // No trained model yet — use rule-based
        if (_predictionEngine is null)
            return _fallback.ParseReceiptLines(lines);

        var result = new OcrResult { IsSuccessful = true };
        var lineItems = new List<LineItem>();
        decimal total = 0;
        string? merchant = lines.Count > 0 ? lines[0].Trim() : null;

        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i].Trim();
            if (line.Length == 0) continue;

            var features = ExtractFeatures(line, i, lines.Count);
            var prediction = _predictionEngine.Predict(features);

            switch (prediction.PredictedLabel)
            {
                case "Product":
                    var item = TryParseProductLine(line);
                    if (item is not null) lineItems.Add(item);
                    break;

                case "Total":
                    var amount = ExtractAmount(line);
                    if (amount.HasValue) total = amount.Value;
                    break;

                // Subtotal, Tax, Merchant, Noise — skip
                default:
                    break;
            }
        }

        if (total == 0 && lineItems.Count > 0)
            total = lineItems.Sum(li => li.TotalPrice);

        result.LineItems = lineItems;
        result.Total = total;
        result.MerchantName = merchant;
        result.ReceiptDate = DateTime.Today;

        return result;
    }

    // ─── Feature extraction — shared between training and inference ──────────
    public static ReceiptLineData ExtractFeatures(string text, int position, int totalLines)
    {
        return new ReceiptLineData
        {
            Text = text,
            HasPrice = Regex.IsMatch(text, @"\d+\.\d{2}") ? 1f : 0f,
            WordCount = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
            HasQuantity = Regex.IsMatch(text, @"^\d+\s*(x|@|qty)", RegexOptions.IgnoreCase) ? 1f : 0f,
            PositionRatio = totalLines > 0 ? (float)position / totalLines : 0f,
            IsAllCaps = text == text.ToUpper() && text.Any(char.IsLetter) ? 1f : 0f,
            HasDollarSign = text.Contains('$') ? 1f : 0f
        };
    }

    private static LineItem? TryParseProductLine(string line)
    {
        var match = Regex.Match(line, @"^(?<name>.+?)\s+\$?(?<price>\d+\.\d{2})$");
        if (!match.Success) return null;

        var name = Regex.Replace(match.Groups["name"].Value, @"\s+", " ").Trim();
        var price = decimal.Parse(match.Groups["price"].Value);

        if (name.Length < 2) return null;

        return new LineItem
        {
            Name = name,
            Quantity = 1,
            UnitPrice = price,
            RawOcrText = line
        };
    }

    private static decimal? ExtractAmount(string line)
    {
        var matches = Regex.Matches(line, @"\d+\.\d{2}");
        return matches.Count > 0 ? decimal.Parse(matches[^1].Value) : null;
    }
}
