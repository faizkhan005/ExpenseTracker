using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ExpenseTracker.ViewModels;

public partial class ExpensesListViewModel : ObservableObject
{
    private readonly IExpenseService _expenseService;
    private readonly ICategoryService _categoryService;

    public ExpensesListViewModel(IExpenseService expenseService, ICategoryService categoryService)
    {
        _expenseService = expenseService;
        _categoryService = categoryService;

        SelectFilterCommand = new AsyncRelayCommand<CategoryFilterItem>(SelectFilterAsync);
        OpenExpenseCommand = new AsyncRelayCommand<ExpenseDisplayItem>(OpenExpenseAsync);
        ClearSearchCommand = new RelayCommand(() => SearchQuery = string.Empty);
        LoadMoreCommand = new AsyncRelayCommand(LoadMoreAsync);
        ExportCommand = new AsyncRelayCommand(ExportAsync);
    }

    public ICommand SelectFilterCommand { get; }
    public ICommand OpenExpenseCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand LoadMoreCommand { get; }
    public ICommand ExportCommand { get; }

    [ObservableProperty]
    public partial ObservableCollection<CategoryFilterItem> CategoryFilters { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<ExpenseDayGroup> GroupedExpenses { get; set; } = [];

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial decimal MonthTotal { get; set; }

    [ObservableProperty]
    public partial int TransactionCount { get; set; }

    [ObservableProperty]
    public partial decimal AvgPerDay { get; set; }

    public bool HasSearchQuery => !string.IsNullOrEmpty(SearchQuery);

    partial void OnSearchQueryChanged(string value) => _ = FilterExpensesAsync();

    // Data
    private List<Expense> _allExpenses = [];

    public async Task LoadDataAsync()
    {
        var now = DateTime.Now;
        var start = new DateTime(now.Year, now.Month, 1);
        _allExpenses = await _expenseService.GetExpensesAsync(start, now);

        var categories = await _categoryService.GetAllAsync();
        CategoryFilters = new ObservableCollection<CategoryFilterItem>(
            new[] { new CategoryFilterItem { Name = "All", IsSelected = true, Id = -1 } }
            .Concat(categories.Select(c => new CategoryFilterItem { Name = c.Name, Id = c.Id })));

        await FilterExpensesAsync();
    }

    private async Task FilterExpensesAsync()
    {
        var selected = CategoryFilters.FirstOrDefault(f => f.IsSelected);
        var filtered = _allExpenses.AsEnumerable();

        if (selected?.Id != -1 && selected != null)
            filtered = filtered.Where(e => e.CategoryId == selected.Id);

        if (!string.IsNullOrWhiteSpace(SearchQuery))
            filtered = filtered.Where(e => e.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

        var list = filtered.OrderByDescending(e => e.Date).ToList();
        MonthTotal = list.Sum(e => e.Amount);
        TransactionCount = list.Count;
        AvgPerDay = DateTime.Now.Day > 0 ? MonthTotal / DateTime.Now.Day : 0;

        var groups = list
            .GroupBy(e => e.Date.Date)
            .Select(g => new ExpenseDayGroup(
                g.Key.ToString("dddd, MMMM d"),
                g.Sum(e => e.Amount),
                g.Select(e => new ExpenseDisplayItem(e)).ToList()))
            .ToList();

        GroupedExpenses = new ObservableCollection<ExpenseDayGroup>(groups);
        OnPropertyChanged(nameof(HasSearchQuery));
    }

    private async Task SelectFilterAsync(CategoryFilterItem? item)
    {
        if (item is null) return;
        foreach (var f in CategoryFilters) f.IsSelected = false;
        item.IsSelected = true;
        OnPropertyChanged(nameof(CategoryFilters));
        await FilterExpensesAsync();
    }

    private async Task OpenExpenseAsync(ExpenseDisplayItem? item)
    {
        if (item is null) return;
        await Shell.Current.GoToAsync($"AddExpensePage?expenseId={item.Id}");
    }

    private Task LoadMoreAsync() => Task.CompletedTask; // Pagination hook
    private Task ExportAsync() => Task.CompletedTask; // Wire to export service

}
