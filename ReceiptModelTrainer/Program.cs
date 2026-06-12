// ─── Bootstrap training data — ~150 labeled examples ──────────────────────────
// Format: (text, label, position_in_receipt_0_to_1)
// Labels: Product, Total, Subtotal, Tax, Merchant, Noise

using Microsoft.ML;

var rawData = new List<(string Text, string Label, float Position)>
{
    // ── Merchant headers (position near 0) ──
    ("WALMART SUPERCENTER", "Merchant", 0.0f),
    ("TARGET", "Merchant", 0.0f),
    ("COSTCO WHOLESALE", "Merchant", 0.0f),
    ("TRADER JOE'S", "Merchant", 0.0f),
    ("WHOLE FOODS MARKET", "Merchant", 0.0f),
    ("KROGER", "Merchant", 0.0f),
    ("SAFEWAY", "Merchant", 0.0f),
    ("CVS PHARMACY", "Merchant", 0.0f),
    ("WALGREENS", "Merchant", 0.0f),
    ("ALDI", "Merchant", 0.0f),
 
    // ── Noise lines (addresses, phone numbers, thank yous) ──
    ("123 MAIN ST ANYTOWN USA", "Noise", 0.05f),
    ("(555) 123-4567", "Noise", 0.05f),
    ("THANK YOU FOR SHOPPING", "Noise", 0.95f),
    ("HAVE A NICE DAY", "Noise", 0.95f),
    ("CASHIER: JOHN", "Noise", 0.1f),
    ("REGISTER 04", "Noise", 0.1f),
    ("STORE #1234", "Noise", 0.02f),
    ("VISIT US AT WWW.EXAMPLE.COM", "Noise", 0.97f),
    ("CARD ENDING IN 1234", "Noise", 0.9f),
    ("AUTH CODE: 00123", "Noise", 0.9f),
    ("REF# 0001234567", "Noise", 0.9f),
    ("**** CUSTOMER COPY ****", "Noise", 0.98f),
    ("ITEMS SOLD: 7", "Noise", 0.85f),
 
    // ── Product lines — grocery format "NAME  PRICE" ──
    ("WHOLE MILK 1GAL          3.98", "Product", 0.3f),
    ("LARGE EGGS 12CT          4.48", "Product", 0.32f),
    ("BREAD SARA LEE           3.98", "Product", 0.34f),
    ("BANANAS                  1.24", "Product", 0.36f),
    ("CHICKEN BREAST 2LB       8.74", "Product", 0.38f),
    ("GREEK YOGURT             5.49", "Product", 0.4f),
    ("ORANGE JUICE 64OZ        4.99", "Product", 0.42f),
    ("CHEDDAR CHEESE 8OZ       3.79", "Product", 0.44f),
    ("PASTA SAUCE              2.49", "Product", 0.46f),
    ("GROUND BEEF 1LB          5.99", "Product", 0.48f),
    ("CEREAL HONEY NUT         4.29", "Product", 0.5f),
    ("PAPER TOWELS 6PK         8.99", "Product", 0.52f),
    ("DISH SOAP                3.29", "Product", 0.54f),
    ("TOOTHPASTE               3.49", "Product", 0.56f),
    ("SHAMPOO 12OZ             6.99", "Product", 0.58f),
 
    // ── Product lines with quantity "QTY @ PRICE  TOTAL" ──
    ("WHOLE MILK 1GAL    2 @ 3.98   7.96", "Product", 0.3f),
    ("LARGE EGGS 12CT    1 @ 4.48   4.48", "Product", 0.32f),
    ("BANANAS            3 @ 0.62   1.86", "Product", 0.36f),
    ("APPLES GALA        4 @ 0.75   3.00", "Product", 0.38f),
    ("WATER BOTTLES 24PK 1 @ 5.99   5.99", "Product", 0.4f),
    ("YOGURT CUPS        6 @ 0.89   5.34", "Product", 0.42f),
    ("FROZEN PIZZA       2 @ 6.49  12.98", "Product", 0.44f),
    ("CANNED BEANS       3 @ 1.29   3.87", "Product", 0.46f),
    ("RICE 5LB BAG       1 @ 4.99   4.99", "Product", 0.5f),
    ("PAPER PLATES       1 @ 4.49   4.49", "Product", 0.55f),
 
    // ── Subtotal lines ──
    ("SUBTOTAL                42.16", "Subtotal", 0.85f),
    ("SUB-TOTAL                42.16", "Subtotal", 0.85f),
    ("SUB TOTAL                42.16", "Subtotal", 0.85f),
    ("SUBTOTAL               123.45", "Subtotal", 0.87f),
    ("SUBTOTAL                 8.99", "Subtotal", 0.83f),
 
    // ── Tax lines ──
    ("TAX                       2.95", "Tax", 0.88f),
    ("SALES TAX                 2.95", "Tax", 0.88f),
    ("VAT                       1.50", "Tax", 0.88f),
    ("GST                       0.45", "Tax", 0.88f),
    ("TAX 7.25%                 3.06", "Tax", 0.89f),
    ("CA SALES TAX               1.10", "Tax", 0.88f),
 
    // ── Total lines ──
    ("TOTAL                    45.11", "Total", 0.92f),
    ("GRAND TOTAL              45.11", "Total", 0.92f),
    ("AMOUNT DUE               45.11", "Total", 0.92f),
    ("TOTAL                   126.40", "Total", 0.93f),
    ("TOTAL                    17.57", "Total", 0.9f),
    ("TOTAL DUE                17.57", "Total", 0.9f),
    ("BALANCE DUE              17.57", "Total", 0.91f),
 
    // ── More merchant-specific examples ──
    ("PIZZA HUT", "Merchant", 0.0f),
    ("MCDONALD'S #4521", "Merchant", 0.0f),
    ("CHICK-FIL-A", "Merchant", 0.0f),
    ("SHELL OIL", "Merchant", 0.0f),
    ("CHEVRON STATION", "Merchant", 0.0f),
    ("AMAZON.COM", "Merchant", 0.0f),
 
    // ── Restaurant-style products ──
    ("CHEESEBURGER             8.99", "Product", 0.3f),
    ("FRIES LARGE              3.49", "Product", 0.35f),
    ("COKE 20OZ                2.29", "Product", 0.4f),
    ("CHICKEN SANDWICH         6.49", "Product", 0.32f),
    ("MILKSHAKE                4.99", "Product", 0.38f),
    ("SALAD CAESAR             7.99", "Product", 0.34f),
 
    // ── Gas station products ──
    ("UNLEADED GAS         12.456 GAL @ 3.299  41.10", "Product", 0.4f),
    ("DIESEL FUEL          15.234 GAL @ 3.899  59.40", "Product", 0.4f),
 
    // ── More noise variants ──
    ("=================================", "Noise", 0.6f),
    ("---------------------------------", "Noise", 0.6f),
    ("ORDER #: 12345", "Noise", 0.05f),
    ("DATE: 01/15/2026", "Noise", 0.02f),
    ("TIME: 14:32:00", "Noise", 0.02f),
    ("SURVEY CODE: 123456", "Noise", 0.96f),
    ("RETURN POLICY 30 DAYS", "Noise", 0.94f),
};

// ─── Convert raw data to feature-engineered training data ─────────────────────
var trainingData = rawData.Select(r =>
{
    var features = MlNetReceiptClassifierFeatures.ExtractFeatures(r.Text, r.Position);
    features.Label = r.Label;
    return features;
}).ToList();

// ─── Train the model ────────────────────────────────────────────────────────
var mlContext = new MLContext(seed: 42);

var dataView = mlContext.Data.LoadFromEnumerable(trainingData);

// Split 80/20 for train/test
var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

var pipeline = mlContext.Transforms.Conversion
    .MapValueToKey(nameof(ReceiptLineDataForTraining.Label))
    .Append(mlContext.Transforms.Concatenate("Features",
        nameof(ReceiptLineDataForTraining.HasPrice),
        nameof(ReceiptLineDataForTraining.WordCount),
        nameof(ReceiptLineDataForTraining.HasQuantity),
        nameof(ReceiptLineDataForTraining.PositionRatio),
        nameof(ReceiptLineDataForTraining.IsAllCaps),
        nameof(ReceiptLineDataForTraining.HasDollarSign)))
    .Append(mlContext.Transforms.NormalizeMinMax("Features"))
    .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
    .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

Console.WriteLine("Training model...");
var model = pipeline.Fit(split.TrainSet);

// ─── Evaluate ──────────────────────────────────────────────────────────────────
var predictions = model.Transform(split.TestSet);
var metrics = mlContext.MulticlassClassification.Evaluate(predictions);

Console.WriteLine($"Micro Accuracy: {metrics.MicroAccuracy:P2}");
Console.WriteLine($"Macro Accuracy: {metrics.MacroAccuracy:P2}");
Console.WriteLine($"Log Loss: {metrics.LogLoss:F4}");

// ─── Save model ──────────────────────────────────────────────────────────────
mlContext.Model.Save(model, dataView.Schema, "receipt_classifier.zip");
Console.WriteLine("Model saved to receipt_classifier.zip");
Console.WriteLine("Copy this file to your MAUI project's Resources/Raw/ folder");


// ─── Helper classes (also needed in MAUI project, kept in sync) ───────────────
public class ReceiptLineDataForTraining
{
    public string Text { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public float HasPrice { get; set; }
    public float WordCount { get; set; }
    public float HasQuantity { get; set; }
    public float PositionRatio { get; set; }
    public float IsAllCaps { get; set; }
    public float HasDollarSign { get; set; }
}

public static class MlNetReceiptClassifierFeatures
{
    public static ReceiptLineDataForTraining ExtractFeatures(string text, float position)
    {
        return new ReceiptLineDataForTraining
        {
            Text = text,
            HasPrice = System.Text.RegularExpressions.Regex.IsMatch(text, @"\d+\.\d{2}") ? 1f : 0f,
            WordCount = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
            HasQuantity = System.Text.RegularExpressions.Regex.IsMatch(text, @"^\d+\s*(x|@|qty)", System.Text.RegularExpressions.RegexOptions.IgnoreCase) ? 1f : 0f,
            PositionRatio = position,
            IsAllCaps = text == text.ToUpper() && text.Any(char.IsLetter) ? 1f : 0f,
            HasDollarSign = text.Contains('$') ? 1f : 0f
        };
    }
}
