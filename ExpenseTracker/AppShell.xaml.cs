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
            Routing.RegisterRoute("InsightsPage", typeof(Views.InsightsPage));
            Routing.RegisterRoute("IconPickerPage", typeof(Views.IconPickerPage));
        }
    }
}
