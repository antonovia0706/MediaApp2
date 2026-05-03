using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MediaApp2.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaApp2.Views;

public partial class AddEquipmentView : Window  
{
    private AddEquipmentViewModel? _viewModel;

    public AddEquipmentView()
    {
        InitializeComponent();
        Opened += OnOpened;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        if (DataContext is AddEquipmentViewModel vm)
        {
            _viewModel = vm;
            vm.OnPhotoSelected += async (_) => await SelectPhotoAsync();
            vm.OnCompleted += (success) => { if (success) Close(); };
        }
    }

    private async Task SelectPhotoAsync()
    {
        if (_viewModel == null) return;

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите фотографию",
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Изображения") { Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.webp" } }
            },
            AllowMultiple = false
        });

        if (files?.Any() == true)
        {
            _viewModel.PhotoPath = files[0].Path.LocalPath;
        }
    }
}