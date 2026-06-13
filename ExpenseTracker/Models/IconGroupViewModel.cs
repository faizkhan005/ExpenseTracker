using CommunityToolkit.Mvvm.ComponentModel;
using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Models;

public partial class IconGroupViewModel : ObservableObject
{
    public string Name { get; set; } = string.Empty;
    public List<IconItemViewModel> Icons { get; set; } = new();

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
