using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models;
using ExpenseTracker.Services.Interface;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ExpenseTracker.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IExpenseService _expenseService;
    private readonly IBudgetService _budgetService;

    public DashboardViewModel(IExpenseService expenseService, IBudgetService budgetService)
    {
        _expenseService = expenseService;
        _budgetService = budgetService;

        GoToMonthlyCommand = new AsyncRelayCommand(GoToMonthlyAsync);
        GoToExpensesCommand = new AsyncRelayCommand(GoToExpensesAsync);
        AddExpenseCommand = new AsyncRelayCommand(AddExpenseAsync);
    }

    public DashboardViewModel()
    {
        _expenseService = null!;
        _budgetService = null!;
        LoadDesignTimeData();

        GoToMonthlyCommand = new AsyncRelayCommand(GoToMonthlyAsync);
        GoToExpensesCommand = new AsyncRelayCommand(GoToExpensesAsync);
        AddExpenseCommand = new AsyncRelayCommand(AddExpenseAsync);
    }

    public ICommand GoToMonthlyCommand { get; }
    public ICommand GoToExpensesCommand { get; }
    public ICommand AddExpenseCommand { get; }


    [ObservableProperty]
    private string _greetingName = "Faizan 👋";

    [ObservableProperty]
    private bool _hasNotifications = true;

    // Budget / totals
    [ObservableProperty]
    private decimal _totalSpent;

    [ObservableProperty]
    private decimal _budgetLimit = 3000m;

    [ObservableProperty]
    private decimal _savedThisMonth;

    [ObservableProperty]
    private decimal _avgDailySpend;

    [ObservableProperty]
    private int _daysLeft;

    // Change indicators
    [ObservableProperty]
    private double _savingsChangePercent;

    [ObservableProperty]
    private double _dailySpendChangePercent;

    // Over-budget warning
    [ObservableProperty]
    private bool _isOverBudgetWarningVisible;

    public string TotalSpentFormatted => TotalSpent.ToString("C0");
    public string BudgetLimitFormatted => BudgetLimit.ToString("C0");
    public string SavedThisMonthFormatted => SavedThisMonth.ToString("C0");
    public string AvgDailySpendFormatted => AvgDailySpend.ToString("C0");

    public double BudgetProgress => BudgetLimit == 0 ? 0 : (double)(TotalSpent / BudgetLimit);

    public string BudgetPercentLabel =>
        $"{Math.Round(BudgetProgress * 100)}% used";

    public string BudgetSubtitle =>
        $"of {BudgetLimitFormatted} budget · {DaysLeft} days left";

    public string SavingsChangeLabel =>
        $"{(SavingsChangePercent >= 0 ? "↑" : "↓")} {Math.Abs(SavingsChangePercent):0}% vs last month";

    public string DailySpendChangeLabel =>
        $"{(DailySpendChangePercent >= 0 ? "↑" : "↓")} {Math.Abs(DailySpendChangePercent):0}% vs last month";

    public Color SavingsChangeLabelColor =>
        SavingsChangePercent >= 0 ? Color.FromArgb("#1D9E75") : Color.FromArgb("#E24B4A");

    public Color DailySpendChangeLabelColor =>
       DailySpendChangePercent <= 0 ? Color.FromArgb("#1D9E75") : Color.FromArgb("#E24B4A");

    public string OverBudgetMessage =>
        $"You're on track to exceed your budget by {(TotalSpent - BudgetLimit):C0}. Tap for tips.";

    [ObservableProperty]
    private ObservableCollection<TransactionDisplayItem> _recentTransactions = new();

    [ObservableProperty]
    private ObservableCollection<CategoryLegendItem> _categoryBreakdown = new();

    // ─── LiveCharts2 — Weekly bar chart ──────────────────────────────────────

    public ISeries[] WeeklySeries { get; private set; } = Array.Empty<ISeries>();

    public Axis[] WeeklyXAxes { get; private set; } =
 {
        new Axis
        {
            Labels = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" },
            LabelsPaint = new SolidColorPaint(SKColors.Gray),
            TextSize = 10,
            SeparatorsPaint = null
        }
    };

    public Axis[] WeeklyYAxes { get; private set; } =
    {
        new Axis
        {
            IsVisible = false,
            SeparatorsPaint = null
        }
    };


    public ISeries[] CategorySeries { get; private set; } = Array.Empty<ISeries>();

    public async Task LoadDataAsync()
    {
        var now = DateTime.Now;
        var start = new DateTime(now.Year, now.Month, 1);
        var end = now;

        // Totals
        var expenses = await _expenseService.GetExpensesAsync(start, end);
        TotalSpent = expenses.Sum(e => e.Amount);
        BudgetLimit = await _budgetService.GetMonthlyBudgetAsync(now.Year, now.Month);
        DaysLeft = DateTime.DaysInMonth(now.Year, now.Month) - now.Day;

        // Savings
        var income = await _expenseService.GetIncomeAsync(now.Year, now.Month);
        SavedThisMonth = income - TotalSpent;

        // Avg daily spend
        int daysElapsed = now.Day;
        AvgDailySpend = daysElapsed > 0 ? TotalSpent / daysElapsed : 0;

        // Change vs last month
        var lastMonth = now.AddMonths(-1);
        var lastMonthStart = new DateTime(lastMonth.Year, lastMonth.Month, 1);
        var lastMonthEnd = lastMonthStart.AddMonths(1).AddDays(-1);
        var lastExpenses = await _expenseService.GetExpensesAsync(lastMonthStart, lastMonthEnd);

        var lastSpent = lastExpenses.Sum(e => e.Amount);
        var lastIncome = await _expenseService.GetIncomeAsync(lastMonth.Year, lastMonth.Month);
        var lastSaved = lastIncome - lastSpent;

        SavingsChangePercent = lastSaved != 0 ? (double)((SavedThisMonth - lastSaved) / lastSaved * 100) : 0;
        var lastAvgDaily = lastExpenses.Count > 0 ? lastSpent / DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month) : 0;
        DailySpendChangePercent = lastAvgDaily != 0 ? (double)((AvgDailySpend - lastAvgDaily) / lastAvgDaily * 100) : 0;

        // Over-budget warning
        IsOverBudgetWarningVisible = TotalSpent > BudgetLimit;

        // Recent transactions (last 5)
        var recent = expenses
            .OrderByDescending(e => e.Date)
            .Take(5)
            .Select(e => new TransactionDisplayItem(e))
            .ToList();

        RecentTransactions = new ObservableCollection<TransactionDisplayItem>(recent);

        // Weekly bar chart
        BuildWeeklyChart(expenses, now);

        // Category breakdown
        BuildCategoryChart(expenses);

        // Refresh computed strings
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

    private void BuildWeeklyChart(IEnumerable<Expense> expenses, DateTime now)
    {
        // Build Mon–Sun of the current week
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
        var dailyTotals = Enumerable.Range(0, 7)
            .Select(i =>
            {
                var day = startOfWeek.AddDays(i);
                var total = expenses
                    .Where(e => e.Date.Date == day.Date)
                    .Sum(e => e.Amount);
                return (double)total;
            })
            .ToArray();

        // Highest day gets accent color, rest get light purple
        var maxIndex = Array.IndexOf(dailyTotals, dailyTotals.Max());

        WeeklySeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Values         = dailyTotals,
                Fill           = new SolidColorPaint(SKColor.Parse("#CECBF6")),
                MaxBarWidth    = 24,
                Rx             = 4,
                Ry             = 4,
                DataLabelsPaint = null
            }
        };

        OnPropertyChanged(nameof(WeeklySeries));
    }

    private void BuildCategoryChart(IEnumerable<Expense> expenses)
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
            .GroupBy(e => e.Category?.Name ?? "Other")
            .Select(g => new
            {
                Name = g.Key,
                Total = g.Sum(e => e.Amount),
                Color = categoryColors.GetValueOrDefault(g.Key, "#B4B2A9")
            })
            .OrderByDescending(g => g.Total)
            .ToList();

        var grandTotal = grouped.Sum(g => g.Total);

        CategorySeries = grouped.Select(g => (ISeries)new PieSeries<double>
        {
            Values = new[] { (double)g.Total },
            Fill = new SolidColorPaint(SKColor.Parse(g.Color)),
            OuterRadiusOffset = 0,
            MaxRadialColumnWidth = 18,
            Name = g.Name
        }).ToArray();

        CategoryBreakdown = new ObservableCollection<CategoryLegendItem>(
            grouped.Select(g => new CategoryLegendItem
            {
                Name = g.Name,
                Percentage = grandTotal > 0 ? (int)Math.Round((double)g.Total / (double)grandTotal * 100) : 0,
                Color = Color.FromArgb(g.Color)
            }));

        OnPropertyChanged(nameof(CategorySeries));
    }

    private async Task GoToMonthlyAsync()
        => await Shell.Current.GoToAsync("//InsightsPage");

    private async Task GoToExpensesAsync()
        => await Shell.Current.GoToAsync("//ExpensesPage");

    private async Task AddExpenseAsync()
        => await Shell.Current.GoToAsync("AddExpensePage");

    private void LoadDesignTimeData()
    {
        GreetingName = "Faizan 👋";
        TotalSpent = 1842m;
        BudgetLimit = 3000m;
        SavedThisMonth = 624m;
        AvgDailySpend = 102m;
        DaysLeft = 18;
        SavingsChangePercent = 12;
        DailySpendChangePercent = 8;
        HasNotifications = true;
        IsOverBudgetWarningVisible = false;

        RecentTransactions = new ObservableCollection<TransactionDisplayItem>
        {
            new() { Name = "Walmart Grocery",    CategoryAndTime = "Food · Today 10:24am",  AmountFormatted = "-$84.50",  AmountColor = Color.FromArgb("#E24B4A"), IconBackground = Color.FromArgb("#EAF3DE"), IconColor = Color.FromArgb("#3B6D11"), IconName = "icon_cart.png" },
            new() { Name = "Shell Gas Station",  CategoryAndTime = "Transport · Yesterday",  AmountFormatted = "-$52.00",  AmountColor = Color.FromArgb("#E24B4A"), IconBackground = Color.FromArgb("#E6F1FB"), IconColor = Color.FromArgb("#185FA5"), IconName = "icon_car.png"  },
            new() { Name = "Salary deposit",     CategoryAndTime = "Income · May 25",        AmountFormatted = "+$3,200", AmountColor = Color.FromArgb("#1D9E75"), IconBackground = Color.FromArgb("#E1F5EE"), IconColor = Color.FromArgb("#0F6E56"), IconName = "icon_bank.png" },
        };

        CategoryBreakdown = new ObservableCollection<CategoryLegendItem>
        {
            new() { Name = "Housing",       Percentage = 40, Color = Color.FromArgb("#534AB7") },
            new() { Name = "Food",          Percentage = 16, Color = Color.FromArgb("#1D9E75") },
            new() { Name = "Transport",     Percentage = 11, Color = Color.FromArgb("#EF9F27") },
            new() { Name = "Dining",        Percentage =  8, Color = Color.FromArgb("#E24B4A") },
            new() { Name = "Other",         Percentage = 25, Color = Color.FromArgb("#CECBF6") },
        };

        WeeklySeries = new ISeries[]
       {
            new ColumnSeries<double>
            {
                Values      = new double[] { 30, 95, 45, 80, 110, 140, 20 },
                Fill        = new SolidColorPaint(SKColor.Parse("#CECBF6")),
                MaxBarWidth = 24,
                Rx = 4, Ry = 4
            }
       };

        CategorySeries = new ISeries[]
        {
            new PieSeries<double> { Values = new[] { 40.0 }, Fill = new SolidColorPaint(SKColor.Parse("#534AB7")), MaxRadialColumnWidth = 18 },
            new PieSeries<double> { Values = new[] { 16.0 }, Fill = new SolidColorPaint(SKColor.Parse("#1D9E75")), MaxRadialColumnWidth = 18 },
            new PieSeries<double> { Values = new[] { 11.0 }, Fill = new SolidColorPaint(SKColor.Parse("#EF9F27")), MaxRadialColumnWidth = 18 },
            new PieSeries<double> { Values = new[] { 8.0  }, Fill = new SolidColorPaint(SKColor.Parse("#E24B4A")), MaxRadialColumnWidth = 18 },
            new PieSeries<double> { Values = new[] { 25.0 }, Fill = new SolidColorPaint(SKColor.Parse("#CECBF6")), MaxRadialColumnWidth = 18 },
        };
    }
}
