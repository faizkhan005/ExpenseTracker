using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Views;

public partial class ExpensesListPage : ContentPage
{
    private readonly ExpensesListViewModel _vm;
    public ExpensesListPage(ExpensesListViewModel vm)
	{
		InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadDataAsync();
    }
}