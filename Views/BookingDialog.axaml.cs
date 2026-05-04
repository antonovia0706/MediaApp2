using Avalonia.Controls;
using Avalonia.Interactivity;
using MediaApp2.ViewModels;

namespace MediaApp2.Views;

public partial class BookingDialog : Window
{
    public BookingDialog()
    {
        InitializeComponent();
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void OnBookClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            await vm.BookRoomCommand.ExecuteAsync(null);
            
            // Если бронирование успешно, закрываем окно
            if (vm.StatusMessage.Contains("Забронировано"))
            {
                Close();
            }
        }
    }
}
