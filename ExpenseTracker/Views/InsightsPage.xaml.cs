using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Views;

public partial class InsightsPage : ContentPage
{
    private readonly InsightsViewModel _vm;
    public InsightsPage(InsightsViewModel vm)
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