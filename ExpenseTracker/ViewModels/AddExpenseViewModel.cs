using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ExpenseTracker.ViewModels;

public partial class AddExpenseViewModel : ObservableObject
{
    private readonly IExpenseService _expenseService;
    private readonly ICategoryService _categoryService;

    public AddExpenseViewModel(IExpenseService expenseService, ICategoryService categoryService)
    {
        _expenseService = expenseService;
        _categoryService = categoryService;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new AsyncRelayCommand(CancelAsync);
        SelectCategoryCommand = new RelayCommand<CategorySelectItem>(SelectCategory);
        SetExpenseTypeCommand = new RelayCommand<string>(SetExpenseType);
        SetDateCommand = new RelayCommand<string>(SetDate);
        AttachReceiptCommand = new AsyncRelayCommand(AttachReceiptAsync);
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand SelectCategoryCommand { get; }
    public ICommand SetExpenseTypeCommand { get; }
    public ICommand SetDateCommand { get; }
    public ICommand AttachReceiptCommand { get; }

    [ObservableProperty]
    public partial int EditingExpenseId { get; set; }

    [ObservableProperty]
    public partial string AmountText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Notes { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsExpense { get; set; } = true;

    [ObservableProperty]
    public partial bool IsIncome { get; set; } = false;

    [ObservableProperty]
    public partial bool IsToday { get; set; } = true;

    [ObservableProperty]
    public partial bool IsYesterday { get; set; } = false;

    [ObservableProperty]
    public partial bool IsCustomDate { get; set; } = false;

    [ObservableProperty]
    public partial bool IsRecurring { get; set; } = false;

    [ObservableProperty]
    public partial bool HasReceiptImage { get; set; } = false;

    [ObservableProperty]
    public partial string SelectedFrequency { get; set; } = "Monthly";

    [ObservableProperty]
    public partial DateTime SelectedDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial List<ImageSource> ReceiptImageSource { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<CategorySelectItem> Categories { get; set; } = [];
    public List<string> FrequencyOptions { get; } = ["Daily", "Weekly", "Monthly", "Yearly"];

    public string PageTitle => EditingExpenseId > 0 ? "Edit expense" : "Add expense";
    public string SaveButtonLabel => EditingExpenseId > 0 ? "Update expense" : "Save expense";

    public async Task LoadAsync(int editingExpenseId = 0)
    {
        EditingExpenseId = editingExpenseId;
        var cats = await _categoryService.GetAllAsync();
        Categories = new ObservableCollection<CategorySelectItem>(
            cats.Where(c => c.Name != "Income")
                .Select(c => new CategorySelectItem(c)));

        if (editingExpenseId > 0)
        {
            var expense = await _expenseService.GetExpenseByIdAsync(editingExpenseId);
            if (expense is not null)
            {
                AmountText = expense.Amount.ToString("0.00");
                Description = expense.Name;
                Notes = expense.Notes ?? string.Empty;
                IsExpense = expense.Type == TransactionType.Expense;
                IsIncome = expense.Type == TransactionType.Income;
                SelectedDate = expense.Date;

                var cat = Categories.FirstOrDefault(c => c.Id == expense.CategoryId);
                cat?.IsSelected = true;
            }
        }
    }

    //commands

    private void SelectCategory(CategorySelectItem? item)
    {
        if (item is null) return;
        foreach (var c in Categories) c.IsSelected = false;
        item.IsSelected = true;
    }

    private void SetExpenseType(string? type)
    {
        IsExpense = type == "expense";
        IsIncome = type == "income";
    }

    private void SetDate(string? option)
    {
        IsToday = option == "today";
        IsYesterday = option == "yesterday";
        IsCustomDate = option == "custom";

        SelectedDate = option switch
        {
            "today" => DateTime.Today,
            "yesterday" => DateTime.Today.AddDays(-1),
            _ => SelectedDate
        };
    }

    private async Task SaveAsync()
    {
        if (!decimal.TryParse(AmountText, out var amount) || amount <= 0)
        {
            await Shell.Current.DisplayAlertAsync("Invalid amount", "Please enter a valid amount.", "OK");
            return;
        }

        var selectedCat = Categories.FirstOrDefault(c => c.IsSelected);
        if (selectedCat is null)
        {
            await Shell.Current.DisplayAlertAsync("No category", "Please select a category.", "OK");
            return;
        }

        var expense = new Expense
        {
            Id = EditingExpenseId,
            Name = string.IsNullOrWhiteSpace(Description) ? selectedCat.Name : Description,
            Amount = amount,
            Date = SelectedDate,
            CategoryId = selectedCat.Id,
            Type = IsExpense ? TransactionType.Expense : TransactionType.Income,
            Source = ExpenseSource.Manual,
            Notes = Notes
        };

        if (EditingExpenseId > 0)
            await _expenseService.UpdateExpenseAsync(expense);
        else
            await _expenseService.AddExpenseAsync(expense);

        await Shell.Current.GoToAsync("..");
    }

    private async Task CancelAsync() => await Shell.Current.GoToAsync("//Dashboard");

    private async Task AttachReceiptAsync()
    {
        List<FileResult> result = await MediaPicker.PickPhotosAsync();
        if (result is null) return;
        foreach (var photo in result)
        {
            ReceiptImageSource.Add(ImageSource.FromFile(photo.FullPath));
        }
        HasReceiptImage = true;
        // TODO: pass image stream to IOcrService and populate amount/description
    }

}
