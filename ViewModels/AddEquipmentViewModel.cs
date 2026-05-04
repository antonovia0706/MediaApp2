using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaApp2.Models;
using MediaApp2.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MediaApp2.ViewModels;

public partial class AddEquipmentViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly TagService _tagService;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _tags = string.Empty;
    [ObservableProperty] private string? _photoPath;
    [ObservableProperty] private string _statusMessage = string.Empty;

    // Теги
    [ObservableProperty] private ObservableCollection<TagModel> _selectedTags = new();
    [ObservableProperty] private ObservableCollection<TagModel> _availableTags = new();

    // События для закрытия окна
    public event Action<bool>? OnCompleted;
    public event Action<string>? OnPhotoSelected;

    public AddEquipmentViewModel(IDataService dataService)
    {
        _dataService = dataService;
        _tagService = new TagService();
        LoadTags();
    }

    private void LoadTags()
    {
        var allTags = _tagService.GetAllTags();
        AvailableTags.Clear();
        foreach (var kvp in allTags)
            AvailableTags.Add(new TagModel(kvp.Key, kvp.Value));
    }

    [RelayCommand]
    private void SelectPhoto()
    {
        OnPhotoSelected?.Invoke("open");
    }

    [RelayCommand]
    private void AddTag(TagModel tag)
    {
        if (!SelectedTags.Any(t => t.Name == tag.Name))
        {
            SelectedTags.Add(tag);
            AvailableTags.Remove(tag);
        }
    }

    [RelayCommand]
    private void RemoveTag(TagModel tag)
    {
        SelectedTags.Remove(tag);
        AvailableTags.Add(tag);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            StatusMessage = "❌ Введите название техники";
            return;
        }

        try
        {
            // Копируем фото в папку проекта, если выбрано
            string? savedPhotoPath = null;
            if (!string.IsNullOrWhiteSpace(PhotoPath) && File.Exists(PhotoPath))
            {
                var photosDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "photos");
                Directory.CreateDirectory(photosDir);

                var fileName = Path.GetFileName(PhotoPath);
                var newPath = Path.Combine(photosDir, fileName);
                File.Copy(PhotoPath, newPath, true);
                savedPhotoPath = Path.Combine("photos", fileName);
            }

            // Собираем теги в строку
            var tagsString = string.Join(",", SelectedTags.Select(t => t.Name));

            // Добавляем технику
            var equipment = new Equipment
            {
                Name = Name.Trim(),
                Description = Description.Trim(),
                Tags = tagsString,
                PhotoPath = savedPhotoPath,
                Status = "available"
            };

            await _dataService.AddEquipmentAsync(equipment);
            StatusMessage = "✅ Техника добавлена!";

            OnCompleted?.Invoke(true);
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        OnCompleted?.Invoke(false);
    }

    [RelayCommand]
    private void AddTagFromAvailable(TagModel tag)
    {
        if (tag == null || SelectedTags.Any(t => t.Name == tag.Name)) return;
        
        SelectedTags.Add(tag);
        AvailableTags.Remove(tag);
    }

    [RelayCommand]
    private void RemoveTagFromSelected(TagModel tag)
    {
        if (tag == null) return;
        
        SelectedTags.Remove(tag);
        AvailableTags.Add(tag);
    }

    [RelayCommand]
    private void OpenTagManager()
    {
        // TODO: Открыть окно управления тегами
        StatusMessage = "ℹ️ Управление тегами (в разработке)";
    }
}