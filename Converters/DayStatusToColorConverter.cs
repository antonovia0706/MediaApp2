using Avalonia.Data.Converters;
using Avalonia.Media;
using MediaApp2.Models;
using System;
using System.Globalization;

namespace MediaApp2.Converters;

public class DayStatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DayStatus status)
        {
            return status switch
            {
                DayStatus.Free => new SolidColorBrush(Color.Parse("#27ae60")),      // Зеленый - Свободно
                DayStatus.PartiallyBusy => new SolidColorBrush(Color.Parse("#f1c40f")), // Желтый - Частично занято
                DayStatus.FullyBusy => new SolidColorBrush(Color.Parse("#e74c3c")),   // Красный - Занято полностью
                _ => new SolidColorBrush(Color.Parse("#ecf0f1"))
            };
        }
        return new SolidColorBrush(Color.Parse("#ecf0f1"));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
