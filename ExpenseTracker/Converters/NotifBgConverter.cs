using System.Globalization;

namespace ExpenseTracker.Converters;

public class NotifBgConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true  // IsRead = true
            ? Color.FromArgb("#F5F5FA")
            : Colors.White;
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
