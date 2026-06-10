using System.Globalization;

namespace ExpenseTracker.Converters;

public class TypeTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true
            ? Colors.White
            : Color.FromArgb("#888888");

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
