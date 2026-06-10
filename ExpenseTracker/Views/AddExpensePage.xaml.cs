using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Views;

[QueryProperty(nameof(ExpenseId), "expenseId")]
public partial class AddExpensePage : ContentPage
{
    private readonly AddExpenseViewModel _vm;

    public string ExpenseId
    {
        set
        {
            if (int.TryParse(value, out var id))
                _ = _vm.LoadAsync(id);
        }
    }

    public AddExpensePage(AddExpenseViewModel vm)
	{
		InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_vm.EditingExpenseId == 0)
            await _vm.LoadAsync();
    }
}