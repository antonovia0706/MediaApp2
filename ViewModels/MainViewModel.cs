using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaApp2.Models;
using MediaApp2.Services;
using MediaApp2.Views;  // ← ДОБАВИТЬ ЭТУ СТРОКУ!
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace MediaApp2.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // ... остальной код
}

public partial class MainViewModel : ObservableObject
{
    private readonly IDataService? _dataService;
    private readonly VkService _vkService;

    public User? CurrentUser { get; set; }

    [ObservableProperty]
    private ObservableCollection<VkPost> _newsFeed = new();

    [ObservableProperty]
    private ObservableCollection<Equipment> _equipmentList = new();

    [ObservableProperty]
    private Equipment? _selectedEquipment;

    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private bool _isLoggedIn;

    [ObservableProperty]
    private string _statusMessage = "Загрузка новостей...";

    [ObservableProperty]
    private int _selectedTabIndex;

    // Свойства для бронирования помещений
    [ObservableProperty]
    private ObservableCollection<RoomBooking> _roomBookings = new();

    [ObservableProperty]
    private RoomBooking? _selectedBooking;

    [ObservableProperty]
    private DateTime _bookingDate = DateTime.Today;

    [ObservableProperty]
    private TimeSpan _bookingStartTime = new(9, 0, 0);

    [ObservableProperty]
    private TimeSpan _bookingEndTime = new(18, 0, 0);

    [ObservableProperty]
    private string _bookingPurpose = string.Empty;

    public MainViewModel(VkService vkService, IDataService? dataService = null)
    {
        _vkService = vkService;
        _dataService = dataService;
        IsLoggedIn = false;
        IsAdmin = false;
        _ = LoadNewsAsync();
    }

    [RelayCommand]
    private async Task LoadNewsAsync()
    {
        try
        {
            StatusMessage = "Обновление...";
            var posts = await _vkService.GetWallPostsAsync(10);
            NewsFeed.Clear();
            foreach (var post in posts) NewsFeed.Add(post);
            StatusMessage = $"Загружено {NewsFeed.Count} новостей";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ShowLogin()
    {
        OnLoginRequested?.Invoke();
    }

    [RelayCommand]
    private void Logout()
    {
        CurrentUser = null;
        IsLoggedIn = false;
        IsAdmin = false;
        StatusMessage = "Вы вышли из системы";
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (_dataService == null || SelectedEquipment == null || CurrentUser == null) return;

        var success = await _dataService.CheckoutAsync(SelectedEquipment.Id, CurrentUser.Id);
        StatusMessage = success ? "Техника закреплена" : "Ошибка";
        await LoadEquipmentAsync();
    }

    [RelayCommand]
    private async Task ReturnAsync()
    {
        if (_dataService == null || SelectedEquipment == null) return;

        var success = await _dataService.ReturnAsync(SelectedEquipment.Id);
        StatusMessage = success ? "Техника возвращена" : "Ошибка";
        await LoadEquipmentAsync();
    }

    [RelayCommand]
    private async Task AddEquipmentAsync(string? name)
    {
        if (_dataService == null || string.IsNullOrWhiteSpace(name)) return;

        await _dataService.AddEquipmentAsync(name);
        StatusMessage = $"Добавлено: {name}";
        await LoadEquipmentAsync();
    }

    [RelayCommand]
    private async Task OpenAddEquipmentWindowAsync()
    {
        if (_dataService == null) return;

        // Получаем главное окно
        var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        if (mainWindow == null) return;

        var vm = new AddEquipmentViewModel(_dataService);
        var view = new AddEquipmentView { DataContext = vm };

        await view.ShowDialog(mainWindow);

        // Если техника добавлена успешно, обновляем список
        if (vm.StatusMessage.Contains("✅"))
        {
            StatusMessage = "Техника добавлена!";
            await LoadEquipmentAsync();
        }
    }

    private async Task LoadEquipmentAsync()
    {
        if (_dataService == null) return;

        var list = await _dataService.GetEquipmentAsync();
        EquipmentList.Clear();
        foreach (var eq in list) EquipmentList.Add(eq);
    }

    public event Action? OnLoginRequested;
    // Свойства корзины
    [ObservableProperty] private ObservableCollection<Equipment> _cart = new();
    [ObservableProperty] private int _cartCount;

    [RelayCommand]
    private void ToggleCart(Equipment equipment)
    {
        if (equipment == null) return;

        if (Cart.Contains(equipment))
            Cart.Remove(equipment);
        else
            Cart.Add(equipment);

        CartCount = Cart.Count;
        StatusMessage = CartCount > 0
            ? $"В корзине: {CartCount} поз."
            : "Корзина пуста";
    }

    [RelayCommand]
    private void ClearCart()
    {
        Cart.Clear();
        CartCount = 0;
        StatusMessage = "Корзина очищена";
    }

    // Методы для бронирования помещений
    [RelayCommand]
    private async Task LoadBookingsAsync()
    {
        if (_dataService == null) return;

        var bookings = await _dataService.GetRoomBookingsAsync();
        RoomBookings.Clear();
        foreach (var b in bookings) RoomBookings.Add(b);
        StatusMessage = $"Загружено {RoomBookings.Count} бронирований";
    }

    [RelayCommand]
    private async Task BookRoomAsync()
    {
        if (_dataService == null || CurrentUser == null) return;
        if (string.IsNullOrWhiteSpace(BookingPurpose))
        {
            StatusMessage = "Укажите цель бронирования";
            return;
        }
        if (BookingEndTime <= BookingStartTime)
        {
            StatusMessage = "Время окончания должно быть позже времени начала";
            return;
        }

        var booking = new RoomBooking
        {
            UserId = CurrentUser.Id,
            UserName = CurrentUser.Login,
            Date = BookingDate,
            StartTime = BookingStartTime,
            EndTime = BookingEndTime,
            Purpose = BookingPurpose
        };

        var success = await _dataService.BookRoomAsync(booking);
        StatusMessage = success ? "Забронировано! Ожидайте подтверждения." : "Ошибка: время уже занято";
        
        if (success)
        {
            BookingPurpose = string.Empty;
            await LoadBookingsAsync();
        }
    }

    [RelayCommand]
    private async Task ApproveBookingAsync()
    {
        if (_dataService == null || SelectedBooking == null) return;

        var success = await _dataService.ApproveBookingAsync(SelectedBooking.Id);
        StatusMessage = success ? "Бронь подтверждена" : "Ошибка";
        await LoadBookingsAsync();
    }

    [RelayCommand]
    private async Task RejectBookingAsync()
    {
        if (_dataService == null || SelectedBooking == null) return;

        var success = await _dataService.RejectBookingAsync(SelectedBooking.Id);
        StatusMessage = success ? "Бронь отклонена" : "Ошибка";
        await LoadBookingsAsync();
    }
}