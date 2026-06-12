namespace ExpenseTracker
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("AddExpensePage", typeof(Views.AddExpensePage));
            Routing.RegisterRoute("RecurringExpensesPage", typeof(Views.SettingsPage));
            Routing.RegisterRoute("NotificationsPage", typeof(Views.NotificationsPage));
        }
    }
}
