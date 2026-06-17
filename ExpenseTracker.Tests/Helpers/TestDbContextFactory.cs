using ExpenseTracker.Domain.Entities;
using SQLite;

namespace ExpenseTracker.Tests.Helpers;

//to reproduce the below line press alt and type 205 on ur number keypad with num lock on
// ═══════════════════════════════════════════════════════════════════════════
// Test-only DB context — creates an in-memory SQLite DB for each test
// ═══════════════════════════════════════════════════════════════════════════
//
// NOTE: The production AppDbContext uses FileSystem.AppDataDirectory, which
// is a MAUI Essentials API requiring a running MAUI app context — it throws
// at runtime in a plain xunit test host. This helper creates an in-memory
// connection with the same table setup and seed logic, so repository logic
// can be tested without MAUI.

public static class TestDbContextFactory
{
    /// <summary>
    /// Creates a fresh in-memory SQLite connection with all tables created
    /// and default categories seeded — mirrors AppDbContext's production setup.
    /// </summary>
    public static async Task<SQLiteAsyncConnection> CreateInMemoryAsync()
    {
        var connection = new SQLiteAsyncConnection(":memory:",
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

        await CreateTablesAsync(connection);
        await SeedDefaultCategoriesAsync(connection);

        return connection;
    }

    private static async Task CreateTablesAsync(SQLiteAsyncConnection db)
    {
        await db.CreateTableAsync<Category>();
        await db.CreateTableAsync<SmsRule>();
        await db.CreateTableAsync<SavedLocation>();
        await db.CreateTableAsync<Budget>();
        await db.CreateTableAsync<RecurringExpense>();
        await db.CreateTableAsync<Expense>();
        await db.CreateTableAsync<LineItem>();
    }

    private static async Task SeedDefaultCategoriesAsync(SQLiteAsyncConnection db)
    {
        var defaults = new List<Category>
        {
            new() { Name = "Food",          IconGlyph = "\ue2e7", ColorHex = "#3B6D11", BackgroundHex = "#EAF3DE", IsSystem = true },
            new() { Name = "Transport",     IconGlyph = "\ue531", ColorHex = "#185FA5", BackgroundHex = "#E6F1FB", IsSystem = true },
            new() { Name = "Housing",       IconGlyph = "\ue88a", ColorHex = "#712B13", BackgroundHex = "#FAECE7", IsSystem = true },
            new() { Name = "Dining",        IconGlyph = "\ue56c", ColorHex = "#633806", BackgroundHex = "#FAEEDA", IsSystem = true },
            new() { Name = "Health",        IconGlyph = "\ue548", ColorHex = "#0F6E56", BackgroundHex = "#E1F5EE", IsSystem = true },
            new() { Name = "Subscriptions", IconGlyph = "\ue325", ColorHex = "#534AB7", BackgroundHex = "#EEEDFE", IsSystem = true },
            new() { Name = "Shopping",      IconGlyph = "\ue8cc", ColorHex = "#72243E", BackgroundHex = "#FBEAF0", IsSystem = true },
            new() { Name = "Income",        IconGlyph = "\ue227", ColorHex = "#0F6E56", BackgroundHex = "#E1F5EE", IsSystem = true },
            new() { Name = "Other",         IconGlyph = "\ue5d3", ColorHex = "#5F5E5A", BackgroundHex = "#F1EFE8", IsSystem = true },
        };

        await db.InsertAllAsync(defaults);
    }
}
