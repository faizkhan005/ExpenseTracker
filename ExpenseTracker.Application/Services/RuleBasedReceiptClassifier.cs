using ExpenseTracker.Application.DTO;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using System.Text.RegularExpressions;

namespace ExpenseTracker.Application.Services;

public class RuleBasedReceiptClassifier : IReceiptLineClassifier
{
    // Regex patterns for common receipt line formats
    private static readonly Regex PriceAtEndPattern =
        new(@"^(?<name>.+?)\s+(?<qty>\d+)?\s*(?:@\s*[\d.]+)?\s*\$?(?<price>\d+\.\d{2})$",
            RegexOptions.Compiled);

    private static readonly Regex SimplePricePattern =
        new(@"^(?<name>.+?)\s+\$?(?<price>\d+\.\d{2})$", RegexOptions.Compiled);

    private static readonly Regex TotalKeywords =
        new(@"^(TOTAL|GRAND TOTAL|AMOUNT DUE)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex SubtotalKeywords =
        new(@"^(SUBTOTAL|SUB-TOTAL|SUB TOTAL)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TaxKeywords =
        new(@"^(TAX|SALES TAX|VAT|GST)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex NoisePatterns =
        new(@"^(THANK YOU|CASHIER|REGISTER|RECEIPT|STORE|PHONE|ADDRESS|VISIT|WWW\.|http|CARD|AUTH|REF#|\*+|-+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public OcrResult ParseReceiptLines(List<string> lines)
    {
        var result = new OcrResult { IsSuccessful = true };
        var lineItems = new List<LineItem>();
        decimal total = 0;
        string? merchant = null;

        // Merchant name is typically the first non-empty line
        if (lines.Count > 0)
            merchant = lines[0].Trim();

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0) continue;

            // Skip noise lines
            if (NoisePatterns.IsMatch(line)) continue;

            // Total line
            if (TotalKeywords.IsMatch(line))
            {
                var amount = ExtractLastAmount(line);
                if (amount.HasValue) total = amount.Value;
                continue;
            }

            // Subtotal / tax lines — skip, don't add as line items
            if (SubtotalKeywords.IsMatch(line) || TaxKeywords.IsMatch(line))
                continue;

            // Try to match "Name Qty @ Price  Total" pattern
            var match = PriceAtEndPattern.Match(line);
            if (match.Success)
            {
                var name = CleanProductName(match.Groups["name"].Value);
                var qty = match.Groups["qty"].Success ? int.Parse(match.Groups["qty"].Value) : 1;
                var price = decimal.Parse(match.Groups["price"].Value);

                if (name.Length > 1 && qty > 0 && qty < 100) // sanity check
                {
                    lineItems.Add(new LineItem
                    {
                        Name = name,
                        Quantity = qty,
                        UnitPrice = qty > 0 ? price / qty : price,
                        RawOcrText = line
                    });
                    continue;
                }
            }

            // Fall back to simple "Name  Price" pattern
            var simpleMatch = SimplePricePattern.Match(line);
            if (simpleMatch.Success)
            {
                var name = CleanProductName(simpleMatch.Groups["name"].Value);
                var price = decimal.Parse(simpleMatch.Groups["price"].Value);

                if (name.Length > 1)
                {
                    lineItems.Add(new LineItem
                    {
                        Name = name,
                        Quantity = 1,
                        UnitPrice = price,
                        RawOcrText = line
                    });
                }
            }
        }

        // If no explicit total found, sum line items
        if (total == 0 && lineItems.Count > 0)
            total = lineItems.Sum(i => i.TotalPrice);

        result.LineItems = lineItems;
        result.Total = total;
        result.MerchantName = merchant;
        result.ReceiptDate = DateTime.Today; // could extract from date patterns too

        return result;
    }

    private static decimal? ExtractLastAmount(string line)
    {
        var matches = Regex.Matches(line, @"\d+\.\d{2}");
        if (matches.Count == 0) return null;
        return decimal.Parse(matches[^1].Value);
    }

    private static string CleanProductName(string name)
    {
        // Remove trailing quantity markers, extra whitespace
        name = Regex.Replace(name, @"\s+", " ").Trim();
        name = Regex.Replace(name, @"\s*\d+\s*@.*$", "").Trim();
        return name;
    }
}
