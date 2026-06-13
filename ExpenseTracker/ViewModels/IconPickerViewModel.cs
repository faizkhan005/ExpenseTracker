using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ExpenseTracker.ViewModels;

public partial class IconPickerViewModel : ObservableObject
{
    private readonly ICategoryService _categoryService;

    public IconPickerViewModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
        SelectGroupCommand = new RelayCommand<IconGroupViewModel>(SelectGroup);
        SelectIconCommand = new RelayCommand<IconItemViewModel>(SelectIcon);
        ClearSearchCommand = new RelayCommand(() => SearchQuery = string.Empty);
        ConfirmCommand = new AsyncRelayCommand(ConfirmAsync);
        CancelCommand = new AsyncRelayCommand(CancelAsync);
    }

    // Commands 
    public ICommand SelectGroupCommand { get; }
    public ICommand SelectIconCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand { get; }

    // Properties passed in via QueryProperty 
    public string CategoryName { get; set; } = string.Empty;

    // Observable 
    [ObservableProperty]
    public partial ObservableCollection<IconGroupViewModel> Groups { get; set; } = [];
    [ObservableProperty]
    public partial ObservableCollection<IconItemViewModel> FilteredIcons { get; set; } = [];
    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;
    [ObservableProperty] 
    public partial bool HasSearchQuery { get; set; } = false;
    [ObservableProperty]
    public partial bool IsSearching { get; set; } = false;
    [ObservableProperty] 
    public partial bool HasSelection { get; set; } = false;
    [ObservableProperty]
    public partial string SelectedGlyph { get; set; } = string.Empty;
    [ObservableProperty] 
    public partial string SelectedLabel { get; set; } = string.Empty;
    [ObservableProperty] 
    public partial Color SelectedIconColor { get; set; } = Color.FromArgb("#534AB7");
    [ObservableProperty]
    public partial Color SelectedBackgroundColor { get; set; } = Color.FromArgb("#EEEDFE");

    private IconItemViewModel? _selectedItem;
    private List<IconItemViewModel> _allIcons = [];

    partial void OnSearchQueryChanged(string value)
    {
        HasSearchQuery = !string.IsNullOrEmpty(value);
        IsSearching = HasSearchQuery;
        FilterIcons();
    }

    // Initialise 
    public void Initialise()
    {
        var groupData = MaterialIconsPicker.Groups;

        Groups = new ObservableCollection<IconGroupViewModel>(
            groupData.Select((g, i) => new IconGroupViewModel
            {
                Name = g.Name,
                IsSelected = i == 0,
                Icons = [.. g.Icons.Select(icon => new IconItemViewModel
                {
                    Glyph = icon.Glyph,
                    Label = icon.Label
                })]
            }));

        _allIcons = [.. Groups.SelectMany(g => g.Icons)];

        // Show first group by default
        ShowGroup(Groups.First());
    }

    // Group selection 
    private void SelectGroup(IconGroupViewModel? group)
    {
        if (group is null) return;

        foreach (var g in Groups) g.IsSelected = false;
        group.IsSelected = true;

        OnPropertyChanged(nameof(Groups));
        ShowGroup(group);
    }

    private void ShowGroup(IconGroupViewModel group)
    {
        FilteredIcons = new ObservableCollection<IconItemViewModel>(group.Icons);
    }

    // Search 
    private void FilterIcons()
    {
        if (!IsSearching)
        {
            var selectedGroup = Groups.FirstOrDefault(g => g.IsSelected);
            if (selectedGroup is not null)
                ShowGroup(selectedGroup);
            return;
        }

        var query = SearchQuery.ToLower();
        var filtered = _allIcons
            .Where(i => i.Label.ToLower().Contains(query))
            .ToList();

        FilteredIcons = new ObservableCollection<IconItemViewModel>(filtered);
    }

    //  Icon selection 
    private void SelectIcon(IconItemViewModel? item)
    {
        if (item is null) return;

        // Deselect previous
        if (_selectedItem is not null)
            _selectedItem.IsSelected = false;

        item.IsSelected = true;
        _selectedItem = item;

        // Update preview
        SelectedGlyph = item.Glyph;
        SelectedLabel = item.Label;
        SelectedIconColor = Color.FromArgb("#534AB7");
        SelectedBackgroundColor = Color.FromArgb("#EEEDFE");
        HasSelection = true;

        // Refresh the visible collection to show selection state
        var temp = FilteredIcons.ToList();
        FilteredIcons = new ObservableCollection<IconItemViewModel>(temp);
    }

    //  Confirm — save category and navigate back 
    private async Task ConfirmAsync()
    {
        if (!HasSelection || string.IsNullOrWhiteSpace(CategoryName)) return;

        var category = new Category
        {
            Name = CategoryName,
            IconGlyph = SelectedGlyph,
            ColorHex = "#534AB7",
            BackgroundHex = "#EEEDFE",
            IsSystem = false
        };

        await _categoryService.AddAsync(category);
        await Shell.Current.GoToAsync("..");
    }

    private async Task CancelAsync() => await Shell.Current.GoToAsync("..");
}


