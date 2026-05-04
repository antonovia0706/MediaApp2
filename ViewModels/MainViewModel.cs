using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaApp2.Models;
using MediaApp2.Services;
using MediaApp2.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System.Globalization;

namespace MediaApp2.ViewModels;

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

    [ObservableProperty]
    private string _guestName = string.Empty;

    [ObservableProperty]
    private string _guestEmail = string.Empty;

    [ObservableProperty]
    private string _guestPhone = string.Empty;

    [ObservableProperty]
    private bool _isGuestBooking = false;

    // Статистика бронирования
    [ObservableProperty]
    private int _totalBookings;

    [ObservableProperty]
    private int _freeDays;

    [ObservableProperty]
    private int _busyDays;

    public MainViewModel(VkService vkService, IDataService? dataService = null)
    {
        _vkService = vkService;
        _dataService = dataService;
        IsLoggedIn = false;
        IsAdmin = false;
        _ = LoadNewsAsync();
        _ = LoadEquipmentAsync();
        _ = LoadCalendarAsync();
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
        
        // Обновляем состояние IsInCart для каждого элемента
        foreach (var eq in list)
        {
            eq.IsInCart = Cart.Any(c => c.Id == eq.Id);
            EquipmentList.Add(eq);
        }
    }

    [RelayCommand]
    private async Task LoadCalendarAsync()
    {
        if (_dataService == null) return;

        var bookings = await _dataService.GetRoomBookingsAsync();
        CalendarDays.Clear();

        // Генерируем дни на текущий месяц (начиная с понедельника)
        var year = SelectedCalendarDate.Year;
        var month = SelectedCalendarDate.Month;
        var firstDayOfMonth = new DateTime(year, month, 1);
        
        // Находим первый понедельник (или первый день месяца, если он понедельник)
        var startDay = firstDayOfMonth;
        while (startDay.DayOfWeek != DayOfWeek.Monday)
        {
            startDay = startDay.AddDays(-1);
        }
        
        // Последний день отображения (6 недель = 42 дня максимум)
        var endDay = startDay.AddDays(41);

        int totalBookingsCount = 0;
        int freeDaysCount = 0;
        int busyDaysCount = 0;

        for (var date = startDay; date <= endDay; date = date.AddDays(1))
        {
            var dayBookings = bookings.Where(b => b.Date.Date == date.Date && b.Status != "rejected").ToList();
            totalBookingsCount += dayBookings.Count;
            
            var bookedHours = dayBookings.Sum(b => (int)(b.EndTime - b.StartTime).TotalHours);
            
            // Определяем статус дня
            DayStatus dayStatus;
            if (!dayBookings.Any())
            {
                dayStatus = DayStatus.Free;
                if (date.Month == month) freeDaysCount++;
            }
            else if (bookedHours >= 8)
            {
                dayStatus = DayStatus.FullyBusy;
                if (date.Month == month) busyDaysCount++;
            }
            else
            {
                dayStatus = DayStatus.PartiallyBusy;
                if (date.Month == month) busyDaysCount++;
            }
            
            var bookingInfo = string.Join("\n", dayBookings.Select(b => 
                $"{b.StartTime:hh\\:mm}-{b.EndTime:hh\\:mm}: {b.Purpose} ({b.UserName})"));

            CalendarDays.Add(new CalendarDay
            {
                Date = date,
                DayNumber = date.Day.ToString(),
                DayName = date.ToString("ddd", new CultureInfo("ru-RU")),
                IsToday = date.Date == DateTime.Today,
                IsCurrentMonth = date.Month == month,
                HasBookings = dayBookings.Any(),
                BookedHours = bookedHours,
                BookingInfo = bookingInfo,
                DayStatus = dayStatus
            });
        }

        // Обновляем статистику
        TotalBookings = totalBookingsCount;
        FreeDays = freeDaysCount;
        BusyDays = busyDaysCount;
    }

    [RelayCommand]
    private void SelectCalendarDay(CalendarDay day)
    {
        if (day == null) return;
        
        SelectedCalendarDate = day.Date;
        BookingDate = day.Date.Date;
        SelectedDayBooking = RoomBookings.FirstOrDefault(b => b.Date.Date == day.Date.Date);
    }

    [RelayCommand]
    private void PreviousMonth()
    {
        SelectedCalendarDate = SelectedCalendarDate.AddMonths(-1);
        _ = LoadCalendarAsync();
    }

    [RelayCommand]
    private void NextMonth()
    {
        SelectedCalendarDate = SelectedCalendarDate.AddMonths(1);
        _ = LoadCalendarAsync();
    }

    public event Action? OnLoginRequested;
    // Свойства корзины
    [ObservableProperty] private ObservableCollection<Equipment> _cart = new();
    [ObservableProperty] private int _cartCount;

    // Свойства для календаря бронирования
    [ObservableProperty] private ObservableCollection<CalendarDay> _calendarDays = new();
    [ObservableProperty] private DateTime _selectedCalendarDate = DateTime.Today;
    [ObservableProperty] private RoomBooking? _selectedDayBooking;

    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public string DayNumber { get; set; } = string.Empty;
        public string DayName { get; set; } = string.Empty;
        public bool IsToday { get; set; }
        public bool IsCurrentMonth { get; set; } = true;
        public bool HasBookings { get; set; }
        public int BookedHours { get; set; }
        public string BookingInfo { get; set; } = string.Empty;
        public DayStatus DayStatus { get; set; } = DayStatus.Free;
    }

    [RelayCommand]
    private void ToggleCart(Equipment equipment)
    {
        if (equipment == null) return;

        if (Cart.Contains(equipment))
        {
            Cart.Remove(equipment);
            equipment.IsInCart = false;
        }
        else
        {
            Cart.Add(equipment);
            equipment.IsInCart = true;
        }

        CartCount = Cart.Count;
        StatusMessage = CartCount > 0
            ? $"В корзине: {CartCount} поз."
            : "Корзина пуста";
    }

    [RelayCommand]
    private void ClearCart()
    {
        foreach (var item in Cart)
            item.IsInCart = false;
        
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
        _ = LoadCalendarAsync(); // Обновляем календарь при загрузке бронирований
    }

    [RelayCommand]
    private async Task BookRoomAsync()
    {
        if (_dataService == null) return;
        
        // Проверка для авторизованных пользователей и гостей
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

        // Проверка на пересечение с существующими бронированиями
        if (_dataService != null)
        {
            var existingBookings = await _dataService.GetRoomBookingsAsync();
            var hasOverlap = existingBookings.Any(b => 
                b.Date.Date == BookingDate.Date && 
                b.Status != "rejected" &&
                !(BookingEndTime <= b.StartTime || BookingStartTime >= b.EndTime));
            
            if (hasOverlap)
            {
                StatusMessage = "Ошибка: выбранное время пересекается с существующим бронированием";
                return;
            }
        }

        string userName;
        int? userId = null;
        bool isGuest = false;
        string guestEmail = string.Empty;
        string guestPhone = string.Empty;

        if (CurrentUser != null)
        {
            // Авторизованный пользователь
            userName = CurrentUser.Login;
            userId = CurrentUser.Id;
        }
        else
        {
            // Гость
            if (string.IsNullOrWhiteSpace(GuestName))
            {
                StatusMessage = "Гость: введите ваше имя";
                return;
            }
            if (string.IsNullOrWhiteSpace(GuestEmail) && string.IsNullOrWhiteSpace(GuestPhone))
            {
                StatusMessage = "Гость: введите email или телефон для связи";
                return;
            }
            
            userName = GuestName;
            isGuest = true;
            guestEmail = GuestEmail;
            guestPhone = GuestPhone;
        }

        var booking = new RoomBooking
        {
            UserId = userId,
            UserName = userName,
            IsGuest = isGuest,
            GuestEmail = guestEmail,
            GuestPhone = guestPhone,
            Date = BookingDate ?? DateTime.Today,
            StartTime = BookingStartTime,
            EndTime = BookingEndTime,
            Purpose = BookingPurpose
        };

        var success = await _dataService.BookRoomAsync(booking);
        StatusMessage = success ? "Забронировано! Ожидайте подтверждения администратора." : "Ошибка: время уже занято";
        
        if (success)
        {
            BookingPurpose = string.Empty;
            if (isGuest)
            {
                GuestName = string.Empty;
                GuestEmail = string.Empty;
                GuestPhone = string.Empty;
            }
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