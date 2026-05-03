using Avalonia.Controls;
using Avalonia.Interactivity;
using MediaApp2.ViewModels;

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
}