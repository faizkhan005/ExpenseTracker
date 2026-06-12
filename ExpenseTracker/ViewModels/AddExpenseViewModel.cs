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
    private readonly IOcrService _ocrService;

    public AddExpenseViewModel(IExpenseService expenseService,
        ICategoryService categoryService,
        IOcrService ocrService)
    {
        _expenseService = expenseService;
        _categoryService = categoryService;
        _ocrService = ocrService;

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
    public partial ImageSource? ReceiptImageSource { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<CategorySelectItem> Categories { get; set; } = [];

    [ObservableProperty]
    public partial bool IsProcessingReceipt { get; set; }

    private List<LineItem> _scannedLineItems = [];

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
            await _expenseService.AddExpenseAsync(expense, _scannedLineItems);

        await Shell.Current.GoToAsync("//Dashboard");
    }

    private async Task CancelAsync() => await Shell.Current.GoToAsync("//Dashboard");

    private async Task AttachReceiptAsync()
    {
        // Ask user which source they want
        var action = await Shell.Current.DisplayActionSheetAsync(
            "Add receipt",
            "Cancel",
            null,
            "Take a photo",
            "Choose from gallery");

        if (action == "Cancel" || action is null) return;

        FileResult? result = null;

        try
        {
            if (action == "Take a photo")
            {
                var status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlert(
                        "Permission needed",
                        "Camera permission is required to take a photo.",
                        "OK");
                    return;
                }

                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await Shell.Current.DisplayAlert(
                        "Not supported",
                        "Camera capture is not supported on this device.",
                        "OK");
                    return;
                }

                // Singular — captures ONE photo. Not deprecated.
                result = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = "Take a receipt photo"
                });
            }
            else
            {
                // Singular — picks ONE photo. Not deprecated.
                // (PickPhotosAsync — plural — is for multi-select and returns List<FileResult>,
                //  which is a different API for a different use case, not a replacement.)
                result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Choose a receipt photo"
                });
            }
        }
        catch (FeatureNotSupportedException)
        {
            await Shell.Current.DisplayAlertAsync("Not supported",
                "This feature is not supported on your device.", "OK");
            return;
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlertAsync("Permission denied",
                "Please grant camera and photo permissions in Settings.", "OK");
            return;
        }

        if (result is null) return;

        // Show image preview immediately
        ReceiptImageSource = ImageSource.FromFile(result.FullPath);
        HasReceiptImage = true;
        IsProcessingReceipt = true;

        try
        {
            // Pass to OCR service
            using var stream = await result.OpenReadAsync();
            var ocrResult = await _ocrService.ScanReceiptAsync(stream);

            if (ocrResult.IsSuccessful)
            {
                // Auto-fill amount if detected
                if (ocrResult.Total > 0)
                    AmountText = ocrResult.Total.ToString("0.00");

                // Auto-fill description from merchant name
                if (!string.IsNullOrWhiteSpace(ocrResult.MerchantName))
                    Description = ocrResult.MerchantName;

                // Store line items
                _scannedLineItems = ocrResult.LineItems;

                if (ocrResult.LineItems.Count > 0)
                    await Shell.Current.DisplayAlertAsync(
                        "Receipt scanned",
                        $"Found {ocrResult.LineItems.Count} items totalling {ocrResult.Total:C}. Amount has been filled in.",
                        "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync(
                    "Could not read receipt",
                    "Please enter the amount manually.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OCR error: {ex.Message}");
            await Shell.Current.DisplayAlertAsync(
                "Scan failed",
                "Could not process the receipt. Please enter the amount manually.",
                "OK");
        }
        finally
        {
            IsProcessingReceipt = false;
        }
    }

}
