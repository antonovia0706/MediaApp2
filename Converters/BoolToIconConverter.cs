using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MediaApp2.Converters;

public class BoolToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? "👤 Гость" : "👤 Пользователь";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}