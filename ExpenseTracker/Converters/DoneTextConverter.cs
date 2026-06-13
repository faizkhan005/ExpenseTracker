using System.Globalization;

namespace ExpenseTracker.Converters;

public class DoneTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? Colors.White : Color.FromArgb("#aaaaaa");
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
