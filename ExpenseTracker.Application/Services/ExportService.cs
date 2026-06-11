using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Services;

public class ExportService : IExportService
{
    private readonly IExpenseService _expenseService;

    public ExportService(IExpenseService expenseService)
        => _expenseService = expenseService;

    public async Task<string> ExportToCsvAsync(int year, int month)
    {
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddSeconds(-1);
        var expenses = await _expenseService.GetExpensesAsync(from, to);
        return BuildCsv(expenses);
    }

    public async Task<string> ExportAllToCsvAsync()
    {
        var expenses = await _expenseService.GetExpensesAsync(
            DateTime.MinValue, DateTime.MaxValue);
        return BuildCsv(expenses);
    }

    private static string BuildCsv(List<Expense> expenses)
    {
        var sb = new System.Text.StringBuilder();

        // Header row
        sb.AppendLine("Date,Name,Category,Amount,Type,Source,Notes");

        foreach (var e in expenses.OrderByDescending(x => x.Date))
        {
            var date = e.Date.ToString("yyyy-MM-dd");
            var name = EscapeCsv(e.Name);
            var category = EscapeCsv(e.Category?.Name ?? "Other");
            var amount = e.Amount.ToString("F2");
            var type = e.Type == TransactionType.Income ? "Income" : "Expense";
            var source = e.Source.ToString();
            var notes = EscapeCsv(e.Notes ?? string.Empty);

            sb.AppendLine($"{date},{name},{category},{amount},{type},{source},{notes}");
        }

        return sb.ToString();
    }
    private static string EscapeCsv(string value)
    {
        // Wrap in quotes if contains comma, quote or newline
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
