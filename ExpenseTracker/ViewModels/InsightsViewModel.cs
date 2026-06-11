using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Application.DTO;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ExpenseTracker.ViewModels;

public partial class InsightsViewModel : ObservableObject
{
    private readonly IIntelligenceService _intelligenceService;
    private readonly IExpenseService _expenseService;
    private readonly IBudgetService _budgetService;

    public InsightsViewModel(
        IIntelligenceService intelligenceService,
        IExpenseService expenseService,
        IBudgetService budgetService)
    {
        _intelligenceService = intelligenceService;
        _expenseService = expenseService;
        _budgetService = budgetService;

        SelectSegmentCommand = new RelayCommand<string>(SelectSegment);
        ExportCommand = new AsyncRelayCommand(ExportAsync);
    }
    // Commands 
    public ICommand SelectSegmentCommand { get; }
    public ICommand ExportCommand { get; }

    // Segment state 
    [ObservableProperty]
    public  partial bool IsOverviewSelected { get; set; } = true;

    [ObservableProperty]
    public partial bool IsCategoriesSelected { get; set; } = false;

    [ObservableProperty]
    public partial bool IsTrendsSelected { get; set; } = false;

    // Overview 
    [ObservableProperty]
    public partial decimal PredictedNextMonth { get; set; }

    [ObservableProperty]
    public partial decimal PotentialSavings { get; set; }

    [ObservableProperty]
    public partial string BiggestCategory { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PredictionSubtitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ISeries[] CategorySeries { get; set; } = Array.Empty<ISeries>();

    [ObservableProperty]
    public partial ObservableCollection<CategoryLegendItem> CategoryBreakdown { get; set; } = [];

    // Categories
    [ObservableProperty]
    public partial ObservableCollection<CategoryBudgetLineDisplay> CategoryBudgetLines { get; set; } = [];

    // Trends
    [ObservableProperty]
    public partial ISeries[] TrendSeries { get; set; } = Array.Empty<ISeries>();

    [ObservableProperty]
    public partial Axis[] TrendXAxes { get; set; } = Array.Empty<Axis>();

    [ObservableProperty]
    public partial Axis[] TrendYAxes { get; set; } = Array.Empty<Axis>();

    [ObservableProperty]
    public partial ObservableCollection<QuantityRecommendation> QuantityRecommendations { get; set; } = [];

    // Tips
    [ObservableProperty]
    public partial ObservableCollection<SavingsTipDisplay> SavingsTips { get; set; } = [];

    [ObservableProperty]
    public partial bool IsLoadingTips { get; set; }

    // Load 
    public async Task LoadDataAsync()
    {
        var now = DateTime.Now;

        PredictedNextMonth = await _intelligenceService.PredictNextMonthSpendAsync();
        var budget = await _budgetService.GetMonthlyBudgetAsync(now.Year, now.Month);
        PotentialSavings = budget > 0 ? budget - PredictedNextMonth : 0;
        PredictionSubtitle = $"Based on 3-month weighted average";

        var byCategory = await _expenseService.GetSpendingByCategoryAsync(now.Year, now.Month);
        BiggestCategory = byCategory.OrderByDescending(k => k.Value).FirstOrDefault().Key ?? "-";

        BuildCategoryCharts(byCategory);
        await BuildTrendChartAsync(now);
        await LoadBudgetLinesAsync(now);
        await LoadTipsAsync(now);

        var qty = await _intelligenceService.GetQuantityRecommendationsAsync();
        QuantityRecommendations = [..qty];
    }

    private void BuildCategoryCharts(Dictionary<string, decimal> byCategory)
    {
        var colors = new Dictionary<string, string>
        {
            ["Food"] = "#534AB7",
            ["Transport"] = "#1D9E75",
            ["Dining"] = "#EF9F27",
            ["Housing"] = "#E24B4A",
            ["Subscriptions"] = "#7F77DD",
            ["Health"] = "#5DCAA5",
            ["Shopping"] = "#D4537E",
            ["Other"] = "#B4B2A9"
        };

        var total = byCategory.Values.Sum();

        CategorySeries = [.. byCategory.Select(kvp => (ISeries)new PieSeries<double>
        {
            Values = [(double)kvp.Value],
            Fill = new SolidColorPaint(SKColor.Parse(colors.GetValueOrDefault(kvp.Key, "#B4B2A9"))),
            MaxRadialColumnWidth = 18,
            Name = kvp.Key
        })];

        CategoryBreakdown = [..
            byCategory.Select(kvp => new CategoryLegendItem
            {
                Name = kvp.Key,
                Percentage = total > 0 ? (int)Math.Round((double)kvp.Value / (double)total * 100) : 0,
                Color = Color.FromArgb(colors.GetValueOrDefault(kvp.Key, "#B4B2A9"))
            })];
    }

    private async Task BuildTrendChartAsync(DateTime now)
    {
        var months = new List<string>();
        var totals = new List<double>();

        for (int i = 5; i >= 0; i--)
        {
            var m = now.AddMonths(-i);
            months.Add(m.ToString("MMM"));
            var spent = await _expenseService.GetTotalSpentAsync(m.Year, m.Month);
            totals.Add((double)spent);
        }

        TrendSeries =
        [
            new LineSeries<double>
            {
                Values      = totals,
                Fill        = new SolidColorPaint(SKColor.Parse("#534AB7").WithAlpha(30)),
                Stroke      = new SolidColorPaint(SKColor.Parse("#534AB7")) { StrokeThickness = 2 },
                GeometrySize = 8,
                GeometryFill = new SolidColorPaint(SKColors.White),
                GeometryStroke = new SolidColorPaint(SKColor.Parse("#534AB7")) { StrokeThickness = 2 }
            }
        ];

        TrendXAxes =
        [
            new Axis { Labels = months, LabelsPaint = new SolidColorPaint(SKColors.Gray), TextSize = 11, SeparatorsPaint = null }
        ];

        TrendYAxes =
        [
            new Axis { IsVisible = false, SeparatorsPaint = null }
        ];
    }

    private async Task LoadBudgetLinesAsync(DateTime now)
    {
        var summary = await _budgetService.GetBudgetSummaryAsync(now.Year, now.Month);
        CategoryBudgetLines = [.. summary.CategoryLines.Select(l => new CategoryBudgetLineDisplay(l))];
    }

    private async Task LoadTipsAsync(DateTime now)
    {
        IsLoadingTips = true;
        var tips = await _intelligenceService.GetSavingsTipsAsync(now.Year, now.Month);
        SavingsTips = [.. tips.Select(t => new SavingsTipDisplay(t))];

        // Load AI tips in background
        _ = Task.Run(async () =>
        {
            var aiTips = await _intelligenceService.GetAiSavingsTipsAsync(now.Year, now.Month);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                foreach (SavingsTip t in aiTips) SavingsTips.Add(new SavingsTipDisplay(t));
                IsLoadingTips = false;
            });
        });
    }

    private void SelectSegment(string? seg)
    {
        IsOverviewSelected = seg == "overview";
        IsCategoriesSelected = seg == "categories";
        IsTrendsSelected = seg == "trends";
    }

    private Task ExportAsync() => Task.CompletedTask;
}
