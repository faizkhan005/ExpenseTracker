using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Views;

[QueryProperty(nameof(CategoryName), "categoryName")]
public partial class IconPickerPage : ContentPage
{
    private readonly IconPickerViewModel _vm;

    public string CategoryName
    {
        set => _vm.CategoryName = value;
    }
    public IconPickerPage(IconPickerViewModel vm)
	{
		InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Initialise();
    }
}