using CommunityToolkit.Mvvm.ComponentModel;

namespace ExpenseTracker.Models;

public partial class IconItemViewModel : ObservableObject
{
    public string Glyph { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
