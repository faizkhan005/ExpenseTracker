using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ExpenseTracker.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IBudgetService _budgetService;
    private readonly ICategoryService _categoryService;
    private readonly IRecurringExpenseService _recurringService;
    private readonly IExportService _exportService;
    private readonly IDeleteDBRepository _deleteDBRepository;

    public SettingsViewModel(
        IBudgetService budgetService,
        ICategoryService categoryService,
        IRecurringExpenseService recurringService,
        IExportService exportService,
        IDeleteDBRepository deleteDBRepository)
    {
        _budgetService = budgetService;
        _categoryService = categoryService;
        _recurringService = recurringService;
        _exportService = exportService;
        _deleteDBRepository = deleteDBRepository;

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
    public partial string MonthlyBudgetText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SavingsGoalText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsSmsParsingEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool IsLocationPromptsEnabled { get; set; } = true;

    [ObservableProperty]
    public partial string RecurringCountLabel { get; set; } = "0 active rules";

    [ObservableProperty]
    public partial ObservableCollection<CategorySelectItem> Categories { get; set; } = [];

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

    private async Task ExportCsvAsync() 
    {
        try
        {
            var now = DateTime.Now;

            // Ask user — this month or all time
            var choice = await Shell.Current.DisplayActionSheetAsync(
                "Export expenses",
                "Cancel",
                null,
                $"This month ({now:MMMM yyyy})",
                "All time");

            if (choice == "Cancel" || choice is null) return;

            string csv = choice.StartsWith("This month")
                ? await _exportService.ExportToCsvAsync(now.Year, now.Month)
                : await _exportService.ExportAllToCsvAsync();

            // Write to a temp file
            var fileName = choice.StartsWith("This month")
                ? $"expenses_{now:yyyy_MM}.csv"
                : "expenses_all.csv";

            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllTextAsync(filePath, csv);

            // Share via native share sheet
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Export expenses",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Export failed", ex.Message, "OK");
        }
    }
    private async Task ClearDataAsync()
    {
        var confirm1 = await Shell.Current.DisplayAlertAsync(
        "Clear all data",
        "This will permanently delete ALL your expenses, budgets and categories. This cannot be undone.",
        "Continue",
        "Cancel");

        if (!confirm1) return;

        var confirm2 = await Shell.Current.DisplayAlertAsync(
            "Are you absolutely sure?",
            "All expense history will be lost forever.",
            "Delete everything",
            "Cancel");

        if (!confirm2) return;

        try
        {
            await _deleteDBRepository.ClearAllDataAsync();

            await Shell.Current.DisplayAlertAsync("Done", "All data has been cleared.", "OK");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }
}
