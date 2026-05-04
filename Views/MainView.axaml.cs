using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using MediaApp2.ViewModels;
using System;

namespace MediaApp2.Views;

public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();
    }

    private async void OnAddClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new TextBox { Watermark = "Введите название техники", Width = 250 };
        var window = new Window
        {
            Content = dialog,
            Title = "Новая техника",
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        await window.ShowDialog(this);

        if (DataContext is MainViewModel vm && !string.IsNullOrWhiteSpace(dialog.Text))
            await vm.AddEquipmentCommand.ExecuteAsync(dialog.Text);
    }

    private async void OnCalendarDayTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border && border.DataContext is MainViewModel.CalendarDay calendarDay)
        {
            if (DataContext is MainViewModel vm)
            {
                // Устанавливаем выбранную дату из календаря
                vm.SelectedCalendarDate = calendarDay.Date;
                vm.BookingDate = calendarDay.Date.Date;
                
                // Открываем диалог бронирования
                var bookingDialog = new BookingDialog { DataContext = vm };
                await bookingDialog.ShowDialog(this);
            }
        }
    }
}