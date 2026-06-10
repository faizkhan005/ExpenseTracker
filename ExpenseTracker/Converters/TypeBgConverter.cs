using System.Globalization;

namespace ExpenseTracker.Converters;

public class TypeBgConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
     => value is true
            ? Color.FromArgb("#534AB7")
            : Color.FromArgb("#F1EFF8");

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
