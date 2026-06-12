using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Infrastructure.Data;

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
                var dbContext = Service?.GetService<AppDbContext>()
                    ?? throw new InvalidOperationException("AppDbContext not found");
                await dbContext.GetConnectionAsync();

                await ModelDeployment.EnsureModelDeployedAsync();

                var recurring = Service?.GetService<IRecurringExpenseService>()
                    ?? throw new InvalidOperationException("Recurring expense service not found");

                await recurring.ProcessDueRecurringExpensesAsync();
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