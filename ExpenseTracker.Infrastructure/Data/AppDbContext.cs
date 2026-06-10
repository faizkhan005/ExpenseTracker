using ExpenseTracker.Domain.Entities;
using SQLite;
using System.Diagnostics;

namespace ExpenseTracker.Infrastructure.Data;

/// <summary>
/// Manages the SQLite connection and owns table creation / migrations.
/// Register as a singleton in DI.
/// </summary>
public class AppDbContext
{
    private SQLiteAsyncConnection? _connection;

    private readonly string _dbPath;

    private const SQLiteOpenFlags Flags =
        SQLiteOpenFlags.ReadWrite |
        SQLiteOpenFlags.Create |
        SQLiteOpenFlags.SharedCache;

    //constructor recieveing the database path (injected from DI) from the MAUI app
    public AppDbContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    // Connection 

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null)
            return _connection;

        _connection = new SQLiteAsyncConnection(_dbPath, Flags);

        await InitialiseTablesAsync(_connection);
        await SeedDefaultDataAsync(_connection);

        return _connection;
    }

    // Table creation

    private static async Task InitialiseTablesAsync(SQLiteAsyncConnection db)
    {
        try
        {
            await db.CreateTableAsync<Category>();
            await db.CreateTableAsync<Expense>();
            await db.CreateTableAsync<LineItem>();
            await db.CreateTableAsync<RecurringExpense>();
            await db.CreateTableAsync<Budget>();
            await db.CreateTableAsync<SmsRule>();
            await db.CreateTableAsync<SavedLocation>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Message: {ex.Message}");
            Debug.WriteLine($"StackTrace: {ex.StackTrace}");

        }
    }

    // Seed default categories

    private static async Task SeedDefaultDataAsync(SQLiteAsyncConnection db)
    {
        try
        {
            var existingCount = await db.Table<Category>().CountAsync();
            if (existingCount > 0)
                return; // Already seeded

            var defaults = new List<Category>
        {
            new() { Name = "Food",          IconKey = "cart",       ColorHex = "#3B6D11", BackgroundHex = "#EAF3DE", IsSystem = true },
            new() { Name = "Transport",     IconKey = "car",        ColorHex = "#185FA5", BackgroundHex = "#E6F1FB", IsSystem = true },
            new() { Name = "Housing",       IconKey = "home",       ColorHex = "#712B13", BackgroundHex = "#FAECE7", IsSystem = true },
            new() { Name = "Dining",        IconKey = "fork",       ColorHex = "#633806", BackgroundHex = "#FAEEDA", IsSystem = true },
            new() { Name = "Health",        IconKey = "health",     ColorHex = "#0F6E56", BackgroundHex = "#E1F5EE", IsSystem = true },
            new() { Name = "Subscriptions", IconKey = "phone",      ColorHex = "#534AB7", BackgroundHex = "#EEEDFE", IsSystem = true },
            new() { Name = "Shopping",      IconKey = "bag",        ColorHex = "#72243E", BackgroundHex = "#FBEAF0", IsSystem = true },
            new() { Name = "Income",        IconKey = "bank",       ColorHex = "#0F6E56", BackgroundHex = "#E1F5EE", IsSystem = true },
            new() { Name = "Other",         IconKey = "dots",       ColorHex = "#5F5E5A", BackgroundHex = "#F1EFE8", IsSystem = true },
        };

            await db.InsertAllAsync(defaults);

            // Seed default SMS rules for common US banks (Android)
            var foodCategoryId = (await db.Table<Category>().Where(c => c.Name == "Food").FirstOrDefaultAsync())?.Id ?? 1;

            var smsRules = new List<SmsRule>
        {
            new()
            {
                BankName          = "Bank of America",
                Pattern           = @"A charge of \$(?<amount>[\d,]+\.?\d*) at (?<merchant>.+?) has been",
                AmountGroup       = "amount",
                MerchantGroup     = "merchant",
                DefaultCategoryId = foodCategoryId,
                IsActive          = true
            },
            new()
            {
                BankName          = "Chase",
                Pattern           = @"A \$(?<amount>[\d,]+\.?\d*) transaction at (?<merchant>.+?) on your",
                AmountGroup       = "amount",
                MerchantGroup     = "merchant",
                DefaultCategoryId = foodCategoryId,
                IsActive          = true
            },
            new()
            {
                BankName          = "Wells Fargo",
                Pattern           = @"Debit card purchase.*?\$(?<amount>[\d,]+\.?\d*).*?(?<merchant>[A-Z][A-Z\s]+)",
                AmountGroup       = "amount",
                MerchantGroup     = "merchant",
                DefaultCategoryId = foodCategoryId,
                IsActive          = true
            },
        };

            await db.InsertAllAsync(smsRules);
        }
        catch (Exception ex) 
        {
            Console.WriteLine(ex.Message);
        }
    }

    //Close
    public async Task CloseAsync()
    {
        if (_connection is not null)
        {
            await _connection.CloseAsync();
            _connection = null;
        }
    }
}
