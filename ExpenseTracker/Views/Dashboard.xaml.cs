using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Views;

public partial class Dashboard : ContentPage
{
	public Dashboard(DashboardViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DashboardViewModel vm)
            await vm.LoadDataAsync();
    }
}