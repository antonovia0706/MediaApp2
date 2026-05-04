using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;
using MediaApp2.Models;

namespace MediaApp2.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "#27ae60" : "#e74c3c";
        }
        return "#95a5a6";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
