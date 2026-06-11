namespace ExpenseTracker.Application.Interfaces;

public interface IExportService
{
    Task<string> ExportToCsvAsync(int year, int month);
    Task<string> ExportAllToCsvAsync();
}
