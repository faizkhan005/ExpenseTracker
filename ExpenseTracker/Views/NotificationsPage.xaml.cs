using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Views;

public partial class NotificationsPage : ContentPage
{
    private readonly NotificationsViewModel _vm;
    public NotificationsPage(NotificationsViewModel vm)
	{
		InitializeComponent();
        BindingContext = _vm = vm;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Load();
    }
}