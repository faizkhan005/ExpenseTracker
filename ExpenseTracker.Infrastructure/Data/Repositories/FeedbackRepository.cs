using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Infrastructure.Data.Repositories;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly AppDbContext _context;

    public FeedbackRepository(AppDbContext context)
        => _context = context;

    public async Task LogCorrectionAsync(string text, string correctedLabel, float positionRatio)
    {
        var db = await _context.GetConnectionAsync();
        await db.CreateTableAsync<ReceiptLineFeedback>(); // safe if exists

        await db.InsertAsync(new ReceiptLineFeedback
        {
            Text = text,
            CorrectedLabel = correctedLabel,
            PositionRatio = positionRatio
        });
    }

    public async Task<List<ReceiptLineFeedback>> GetAllAsync()
    {
        var db = await _context.GetConnectionAsync();
        await db.CreateTableAsync<ReceiptLineFeedback>();
        return await db.Table<ReceiptLineFeedback>().ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        var db = await _context.GetConnectionAsync();
        await db.CreateTableAsync<ReceiptLineFeedback>();
        return await db.Table<ReceiptLineFeedback>().CountAsync();
    }

    /// <summary>
    /// Export collected feedback as CSV — paste into TrainModel.cs rawData
    /// list (as additional tuples) when you have enough examples to retrain.
    /// </summary>
    public async Task<string> ExportAsCsvAsync()
    {
        var all = await GetAllAsync();
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Text,Label,Position");

        foreach (var f in all)
            sb.AppendLine($"\"{f.Text.Replace("\"", "\"\"")}\",{f.CorrectedLabel},{f.PositionRatio:F2}");

        return sb.ToString();
    }
}
