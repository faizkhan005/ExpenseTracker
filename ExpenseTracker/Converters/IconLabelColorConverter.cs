using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ExpenseTracker.Converters;

public class IconLabelColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? Color.FromArgb("#534AB7") : Color.FromArgb("#aaaaaa");
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
