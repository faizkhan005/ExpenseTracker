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
            var recurring = Service?.GetService<IRecurringExpenseService>()?? throw new InvalidOperationException("Recurring expense service not found");
            await recurring.ProcessDueRecurringExpensesAsync();
        }
    }
}