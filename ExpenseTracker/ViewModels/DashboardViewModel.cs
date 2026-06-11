using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

namespace ExpenseTracker.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    // Dependencies 
    private readonly IExpenseService _expenseService;
    private readonly IBudgetService _budgetService;

    // Constructor 
    public DashboardViewModel(IExpenseService expenseService, IBudgetService budgetService)
    {
        _expenseService = expenseService;
        _budgetService = budgetService;
        GoToMonthlyCommand = new AsyncRelayCommand(GoToMonthlyAsync);
        GoToExpensesCommand = new AsyncRelayCommand(GoToExpensesAsync);
        AddExpenseCommand = new AsyncRelayCommand(AddExpenseAsync);
    }

    // Commands 
    public ICommand GoToMonthlyCommand { get; }
    public ICommand GoToExpensesCommand { get; }
    public ICommand AddExpenseCommand { get; }

    // Observable properties
    [ObservableProperty]
    public partial string GreetingName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasNotifications { get; set; } = false;

    [ObservableProperty]
    public partial decimal TotalSpent { get; set; }

    [ObservableProperty]
    public partial decimal BudgetLimit { get; set; }

    [ObservableProperty]
    public partial decimal SavedThisMonth { get; set; }

    [ObservableProperty]
    public partial decimal AvgDailySpend { get; set; }

    [ObservableProperty]
    public partial int DaysLeft { get; set; }

    [ObservableProperty]
    public partial double SavingsChangePercent { get; set; }

    [ObservableProperty]
    public partial double DailySpendChangePercent { get; set; }

    [ObservableProperty]
    public partial bool IsOverBudgetWarningVisible { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<TransactionDisplayItem> RecentTransactions { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<CategoryLegendItem> CategoryBreakdown { get; set; } = [];

    // LiveCharts2
    [ObservableProperty]
    public partial ISeries[] WeeklySeries { get; private set; } = [];
    [ObservableProperty]
    public partial ISeries[] CategorySeries { get; private set; } = [];

    // Computed display strings
    public string TotalSpentFormatted => TotalSpent.ToString("C0", CultureInfo.CreateSpecificCulture("en-US"));
    public string BudgetLimitFormatted => BudgetLimit.ToString("C0", CultureInfo.CreateSpecificCulture("en-US"));
    public string SavedThisMonthFormatted => SavedThisMonth.ToString("C0", CultureInfo.CreateSpecificCulture("en-US"));
    public string AvgDailySpendFormatted => AvgDailySpend.ToString("C0", CultureInfo.CreateSpecificCulture("en-US"));

    public double BudgetProgress => BudgetLimit == 0
        ? 0
        : Math.Min(1.0, (double)(TotalSpent / BudgetLimit));

    public string BudgetPercentLabel =>
        $"{Math.Round(BudgetProgress * 100)}% used";

    public string BudgetSubtitle =>
        BudgetLimit > 0
            ? $"of {BudgetLimitFormatted} budget · {DaysLeft} days left"
            : $"{DaysLeft} days left this month";

    public string SavingsChangeLabel =>
        $"{(SavingsChangePercent >= 0 ? "↑" : "↓")} {Math.Abs(SavingsChangePercent):0}% vs last month";

    public string DailySpendChangeLabel =>
        $"{(DailySpendChangePercent >= 0 ? "↑" : "↓")} {Math.Abs(DailySpendChangePercent):0}% vs last month";

    public Color SavingsChangeLabelColor =>
       SavingsChangePercent >= 0
           ? Color.FromArgb("#1D9E75")
           : Color.FromArgb("#E24B4A");

    public Color DailySpendChangeLabelColor =>
        DailySpendChangePercent <= 0
            ? Color.FromArgb("#1D9E75")
            : Color.FromArgb("#E24B4A");

    public string OverBudgetMessage =>
        $"You are on track to exceed your budget by {(TotalSpent - BudgetLimit):C0}. Tap for tips.";

    

    public Axis[] WeeklyXAxes { get; private set; } =
    [
        new Axis
        {
            Labels          = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"],
            LabelsPaint     = new SolidColorPaint(SKColors.Gray),
            TextSize        = 10,
            SeparatorsPaint = null
        }
    ];

    public Axis[] WeeklyYAxes { get; private set; } =
    [
        new Axis { IsVisible = false, SeparatorsPaint = null }
    ];

    // Main data load 
    public async Task LoadDataAsync()
    {
        IsLoading = true;

        var now = DateTime.Now;
        var start = new DateTime(now.Year, now.Month, 1);

        // Greeting changes based on time of day
        var hour = now.Hour;
        GreetingName = hour < 12 ? "Good morning" :
                       hour < 17 ? "Good afternoon" :
                                   "Good evening";

        // Current month expenses from SQLite via service
        var expenses = await _expenseService.GetExpensesAsync(start, now);

        TotalSpent = expenses
            .Where(e => e.Type == TransactionType.Expense)
            .Sum(e => e.Amount);

        // Budget from DB — 0 if not set yet
        BudgetLimit = await _budgetService.GetMonthlyBudgetAsync(now.Year, now.Month);
        DaysLeft = DateTime.DaysInMonth(now.Year, now.Month) - now.Day;

        // Savings = income logged this month minus spending
        var income = expenses
            .Where(e => e.Type == TransactionType.Income)
            .Sum(e => e.Amount);
        SavedThisMonth = income - TotalSpent;

        // Average daily spend across days elapsed so far
        int daysElapsed = Math.Max(1, now.Day);
        AvgDailySpend = TotalSpent / daysElapsed;

        // Compare against last month for change indicators
        var lastMonth = now.AddMonths(-1);
        var lastMonthStart = new DateTime(lastMonth.Year, lastMonth.Month, 1);
        var lastMonthEnd = lastMonthStart.AddMonths(1).AddSeconds(-1);
        var lastExpenses = await _expenseService.GetExpensesAsync(lastMonthStart, lastMonthEnd);

        var lastSpent = lastExpenses.Where(e => e.Type == TransactionType.Expense).Sum(e => e.Amount);
        var lastIncome = lastExpenses.Where(e => e.Type == TransactionType.Income).Sum(e => e.Amount);
        var lastSaved = lastIncome - lastSpent;

        SavingsChangePercent = lastSaved != 0
            ? (double)((SavedThisMonth - lastSaved) / Math.Abs(lastSaved) * 100)
            : 0;

        var lastDaysInMonth = (decimal)DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month);
        var lastAvgDaily = lastSpent / Math.Max(1, lastDaysInMonth);
        DailySpendChangePercent = lastAvgDaily != 0
            ? (double)((AvgDailySpend - lastAvgDaily) / lastAvgDaily * 100)
            : 0;

        // Show warning banner only when actively over budget
        IsOverBudgetWarningVisible = BudgetLimit > 0 && TotalSpent > BudgetLimit;

        // 5 most recent transactions for the list
        RecentTransactions = new ObservableCollection<TransactionDisplayItem>(
            expenses
                .OrderByDescending(e => e.Date)
                .Take(5)
                .Select(e => new TransactionDisplayItem(e)));

        // Build charts from real data
        BuildWeeklyChart(expenses, now);
        BuildCategoryChart(expenses);

        // Refresh all computed string bindings
        NotifyComputedProperties();

        IsLoading = false;
    }

    // Chart builders
    private void BuildWeeklyChart(List<Expense> expenses, DateTime now)
    {
        int diff = ((int)now.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var startOfWeek = now.Date.AddDays(-diff);

        var dailyTotals = Enumerable.Range(0, 7)
            .Select(i =>
            {
                var day = startOfWeek.AddDays(i);
                return (double)expenses
                    .Where(e => e.Date.Date == day && e.Type == TransactionType.Expense)
                    .Sum(e => e.Amount);
            })
            .ToArray();

        WeeklySeries =
        [
            new ColumnSeries<double>
            {
                Values          = dailyTotals,
                Fill            = new SolidColorPaint(SKColor.Parse("#CECBF6")),
                MaxBarWidth     = 24,
                Rx              = 4,
                Ry              = 4,
                DataLabelsPaint = null
            }
        ];

        OnPropertyChanged(nameof(WeeklySeries));
    }

    private void BuildCategoryChart(List<Expense> expenses)
    {
        var categoryColors = new Dictionary<string, string>
        {
            ["Food"] = "#534AB7",
            ["Transport"] = "#1D9E75",
            ["Dining"] = "#EF9F27",
            ["Housing"] = "#E24B4A",
            ["Subscriptions"] = "#7F77DD",
            ["Health"] = "#5DCAA5",
            ["Shopping"] = "#D4537E",
            ["Other"] = "#B4B2A9",
        };

        var grouped = expenses
            .Where(e => e.Type == TransactionType.Expense)
            .GroupBy(e => e.Category?.Name ?? "Other")
            .Select(g => new
            {
                Name = g.Key,
                Total = g.Sum(e => e.Amount),
                Color = categoryColors.GetValueOrDefault(g.Key, "#B4B2A9")
            })
            .Where(g => g.Total > 0)
            .OrderByDescending(g => g.Total)
            .ToList();

        var grandTotal = grouped.Sum(g => g.Total);

        CategorySeries = [.. grouped
            .Select(g => (ISeries)new PieSeries<double>
            {
                Values = [(double)g.Total],
                Fill = new SolidColorPaint(SKColor.Parse(g.Color)),
                MaxRadialColumnWidth = 18,
                Name = g.Name
            })];

        CategoryBreakdown = new ObservableCollection<CategoryLegendItem>(
            grouped.Select(g => new CategoryLegendItem
            {
                Name = g.Name,
                Percentage = grandTotal > 0
                    ? (int)Math.Round((double)g.Total / (double)grandTotal * 100)
                    : 0,
                Color = Color.FromArgb(g.Color)
            }));

        OnPropertyChanged(nameof(CategorySeries));
    }

    // Helpers

    private void NotifyComputedProperties()
    {
        OnPropertyChanged(nameof(TotalSpentFormatted));
        OnPropertyChanged(nameof(BudgetLimitFormatted));
        OnPropertyChanged(nameof(SavedThisMonthFormatted));
        OnPropertyChanged(nameof(AvgDailySpendFormatted));
        OnPropertyChanged(nameof(BudgetProgress));
        OnPropertyChanged(nameof(BudgetPercentLabel));
        OnPropertyChanged(nameof(BudgetSubtitle));
        OnPropertyChanged(nameof(SavingsChangeLabel));
        OnPropertyChanged(nameof(DailySpendChangeLabel));
        OnPropertyChanged(nameof(SavingsChangeLabelColor));
        OnPropertyChanged(nameof(DailySpendChangeLabelColor));
        OnPropertyChanged(nameof(OverBudgetMessage));
    }

    // Navigation 
    private Task GoToMonthlyAsync() => Shell.Current.GoToAsync("//InsightsPage");
    private Task GoToExpensesAsync() => Shell.Current.GoToAsync("//ExpensesPage");
    private Task AddExpenseAsync() => Shell.Current.GoToAsync("AddExpensePage");

}
