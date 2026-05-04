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

public class DayStatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DayStatus status)
        {
            return status switch
            {
                DayStatus.Free => "#27ae60",      // Зеленый - свободно
                DayStatus.PartiallyBusy => "#f1c40f", // Желтый - частично занято
                DayStatus.FullyBusy => "#e74c3c",   // Красный - занято полностью
                _ => "#95a5a6"
            };
        }
        return "#ecf0f1"; // По умолчанию - светло-серый для пустых ячеек
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
