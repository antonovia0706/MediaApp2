using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MediaApp2.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString()?.ToLower() switch
        {
            "available" => "#27ae60",
            "taken" => "#e74c3c",
            _ => "#95a5a6"
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}