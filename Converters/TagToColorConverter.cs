using Avalonia.Data.Converters;
using MediaApp2.Services;
using System;
using System.Globalization;

namespace MediaApp2.Converters;

public class TagToColorConverter : IValueConverter
{
    private static readonly TagService _tagService = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string tag)
            return _tagService.GetTagColor(tag);
        return "#95a5a6";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}