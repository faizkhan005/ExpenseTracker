using System.Globalization;

namespace ExpenseTracker.Converters
{
    public class NegativeAmountColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal amount)
                return amount < 0
                    ? Color.FromArgb("#E24B4A")
                    : Color.FromArgb("#1D9E75");

            return Color.FromArgb("#1a1a2e");
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
