using ExpenseTracker.Application.Interfaces;

namespace ExpenseTracker
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public static IServiceProvider? Service { get; private set; }
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            Service = serviceProvider;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        protected override async void OnStart()
        {
            base.OnStart();
            try
            {
                // Ensure AppDbContext is fully initialized before proceeding
                var dbContext = Service?.GetService<ExpenseTracker.Infrastructure.Data.AppDbContext>()
                    ?? throw new InvalidOperationException("AppDbContext not found");

                // This call ensures the connection and all tables are created
                await dbContext.GetConnectionAsync();

                System.Diagnostics.Debug.WriteLine("✓ AppDbContext initialized successfully");

                // NOW safely process recurring expenses
                var recurring = Service?.GetService<IRecurringExpenseService>()
                    ?? throw new InvalidOperationException("Recurring expense service not found");

                await recurring.ProcessDueRecurringExpensesAsync();

                System.Diagnostics.Debug.WriteLine("✓ Recurring expenses processed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ OnStart Error: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"❌ Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack: {ex.StackTrace}");

                // Don't crash silently - at least log it
               
            }
        }
    }
}