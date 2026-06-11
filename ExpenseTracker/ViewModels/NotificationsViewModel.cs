using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ExpenseTracker.ViewModels;

public partial class NotificationsViewModel : ObservableObject
{
    private readonly IExpenseService _expenseService;
    private readonly IBudgetService _budgetService;

    public NotificationsViewModel(IExpenseService expenseService, IBudgetService budgetService)
    {
        _expenseService = expenseService;
        _budgetService = budgetService;
        BackCommand = new AsyncRelayCommand(() => Shell.Current.GoToAsync(".."));
        MarkAllReadCommand = new RelayCommand(MarkAllRead);
    }

    public ICommand BackCommand { get; }
    public ICommand MarkAllReadCommand { get; }

    [ObservableProperty]
    public partial ObservableCollection<NotificationItem> Notifications { get; set; } = [];

    public void Load() => _ = LoadAsync();

    private async Task LoadAsync()
    {
        var now = DateTime.Now;
        var start = new DateTime(now.Year, now.Month, 1);
        var expenses = await _expenseService.GetExpensesAsync(start, now);
        var budgetLimit = await _budgetService.GetMonthlyBudgetAsync(now.Year, now.Month);
        var totalSpent = expenses.Where(e => e.Type == TransactionType.Expense).Sum(e => e.Amount);
        var items = new List<NotificationItem>();

        // Over budget
        if (budgetLimit > 0 && totalSpent > budgetLimit)
            items.Add(new NotificationItem
            {
                Title = "Over budget",
                Body = $"You've exceeded your {budgetLimit:C0} budget by {(totalSpent - budgetLimit):C0}.",
                IconGlyph = "\ue002",
                IconColor = Color.FromArgb("#712B13"),
                IconBackground = Color.FromArgb("#FAECE7")
            });

        // Approaching 80%
        else if (budgetLimit > 0 && totalSpent >= budgetLimit * 0.8m)
            items.Add(new NotificationItem
            {
                Title = "Approaching budget limit",
                Body = $"You've used {(totalSpent / budgetLimit * 100):0}% of your monthly budget.",
                IconGlyph = "\ue002",
                IconColor = Color.FromArgb("#633806"),
                IconBackground = Color.FromArgb("#FAEEDA")
            });

        // High spending today vs average
        var todaySpend = expenses
            .Where(e => e.Date.Date == DateTime.Today && e.Type == TransactionType.Expense)
            .Sum(e => e.Amount);
        var avgDaily = now.Day > 1 ? totalSpent / now.Day : 0;
        if (avgDaily > 0 && todaySpend > avgDaily * 1.5m)
            items.Add(new NotificationItem
            {
                Title = "High spending today",
                Body = $"You've spent {todaySpend:C0} today — {((todaySpend / avgDaily - 1) * 100):0}% above your daily average.",
                IconGlyph = "\ue8b1",
                IconColor = Color.FromArgb("#185FA5"),
                IconBackground = Color.FromArgb("#E6F1FB")
            });

        // No expenses today
        if (!expenses.Any(e => e.Date.Date == DateTime.Today))
            items.Add(new NotificationItem
            {
                Title = "Don't forget to log",
                Body = "You haven't added any expenses today.",
                IconGlyph = "\ue7f4",
                IconColor = Color.FromArgb("#0F6E56"),
                IconBackground = Color.FromArgb("#E1F5EE")
            });

        // Recurring expenses this month
        var recurringCount = expenses.Count(e => e.Source == ExpenseSource.Recurring);
        if (recurringCount > 0)
            items.Add(new NotificationItem
            {
                Title = $"{recurringCount} recurring expense{(recurringCount > 1 ? "s" : "")} logged",
                Body = "Your scheduled expenses were automatically added this month.",
                IconGlyph = "\ue042",
                IconColor = Color.FromArgb("#633806"),
                IconBackground = Color.FromArgb("#FAEEDA")
            });

        Notifications = new ObservableCollection<NotificationItem>(items);
    }

    private void MarkAllRead()
    {
        foreach (var n in Notifications)
            n.IsRead = true;
        var temp = Notifications.ToList();
        Notifications = new ObservableCollection<NotificationItem>(temp);
    }
}
