using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using static ExpenseTracker.ViewModels.AddExpenseViewModel;

namespace ExpenseTracker.ViewModels;
public partial class SettingsViewModel : ObservableObject
{
    private readonly IBudgetService _budgetService;
    private readonly ICategoryService _categoryService;
    private readonly IRecurringExpenseService _recurringService;

    public SettingsViewModel(
        IBudgetService budgetService,
        ICategoryService categoryService,
        IRecurringExpenseService recurringService)
    {
        _budgetService = budgetService;
        _categoryService = categoryService;
        _recurringService = recurringService;

        SaveBudgetCommand = new AsyncRelayCommand(SaveBudgetAsync);
        ManageRecurringCommand = new AsyncRelayCommand(ManageRecurringAsync);
        AddCategoryCommand = new AsyncRelayCommand(AddCategoryAsync);
        DeleteCategoryCommand = new AsyncRelayCommand<CategorySelectItem>(DeleteCategoryAsync);
        ExportCsvCommand = new AsyncRelayCommand(ExportCsvAsync);
        ClearDataCommand = new AsyncRelayCommand(ClearDataAsync);
    }

    //Commands 
    public ICommand SaveBudgetCommand { get; }
    public ICommand ManageRecurringCommand { get; }
    public ICommand AddCategoryCommand { get; }
    public ICommand DeleteCategoryCommand { get; }
    public ICommand ExportCsvCommand { get; }
    public ICommand ClearDataCommand { get; }

    // Observable z
    [ObservableProperty]
    private partial string MonthlyBudgetText { get; set; } = string.Empty;

    [ObservableProperty]
    private partial string SavingsGoalText { get; set; } = string.Empty;

    [ObservableProperty]
    private partial bool IsSmsParsingEnabled { get; set; } = true;

    [ObservableProperty]
    private partial bool IsLocationPromptsEnabled { get; set; } = true;

    [ObservableProperty]
    private partial string RecurringCountLabel { get; set; } = "0 active rules";

    [ObservableProperty]
    private partial ObservableCollection<CategorySelectItem> Categories { get; set; } = [];

    public async Task LoadDataAsync()
    {
        var now = DateTime.Now;
        var budget = await _budgetService.GetMonthlyBudgetAsync(now.Year, now.Month);
        MonthlyBudgetText = budget.ToString("0");

        var cats = await _categoryService.GetAllAsync();
        Categories = new ObservableCollection<CategorySelectItem>(cats.Select(c => new CategorySelectItem(c)));

        var recurring = await _recurringService.GetAllAsync();
        RecurringCountLabel = $"{recurring.Count} active rule{(recurring.Count != 1 ? "s" : "")}";
    }

    private async Task SaveBudgetAsync()
    {
        if (!decimal.TryParse(MonthlyBudgetText, out var amount)) return;
        var now = DateTime.Now;
        await _budgetService.SetMonthlyBudgetAsync(now.Year, now.Month, amount);
        await Shell.Current.DisplayAlertAsync("Saved", "Budget updated successfully.", "OK");
    }

    private Task ManageRecurringAsync()
        => Shell.Current.GoToAsync("RecurringExpensesPage");

    private async Task AddCategoryAsync()
    {
        var name = await Shell.Current.DisplayPromptAsync("New category", "Enter category name:");
        if (string.IsNullOrWhiteSpace(name)) return;

        var category = new Category { Name = name, IconKey = "dots", ColorHex = "#534AB7", BackgroundHex = "#EEEDFE" };
        await _categoryService.AddAsync(category);
        await LoadDataAsync();
    }

    private async Task DeleteCategoryAsync(CategorySelectItem? item)
    {
        if (item is null || item.IsSelected) return;
        var confirm = await Shell.Current.DisplayAlertAsync("Delete", $"Delete '{item.Name}'?", "Delete", "Cancel");
        if (!confirm) return;
        await _categoryService.DeleteAsync(item.Id);
        await LoadDataAsync();
    }

    private Task ExportCsvAsync() => Task.CompletedTask; // Wire to export service
    private async Task ClearDataAsync()
    {
        var confirm = await Shell.Current.DisplayAlertAsync("Clear data", "This will permanently delete all expenses. Are you sure?", "Delete all", "Cancel");
        if (!confirm) return;
        // Wire to a clear-data service
    }
}
