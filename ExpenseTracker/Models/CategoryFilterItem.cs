using CommunityToolkit.Mvvm.ComponentModel;

namespace ExpenseTracker.Models
{
    public partial class CategoryFilterItem : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
