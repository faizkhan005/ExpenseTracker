using CommunityToolkit.Mvvm.ComponentModel;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Models;

public partial class CategorySelectItem : ObservableObject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public string BackgroundHex { get; set; } = string.Empty;
    public string IconGlyph { get; set; } = string.Empty;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public CategorySelectItem() { }
    public CategorySelectItem(Category c)
    {
        Id = c.Id;
        Name = c.Name;
        ColorHex = c.ColorHex;
        BackgroundHex = c.BackgroundHex;
        IconGlyph = CategoryIconMap.GetGlyph(c.IconKey);
    }
}
