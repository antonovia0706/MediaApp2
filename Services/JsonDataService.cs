using MediaApp2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MediaApp2.Services;

public class JsonDataService : IDataService
{
    private readonly string _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "media_db.json");
    private List<User> _users = new();
    private List<Equipment> _equipment = new();
    private List<HistoryRecord> _history = new();
    private List<RoomBooking> _bookings = new();

    // Настройки сериализации: игнорировать регистр имён свойств
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public JsonDataService() => LoadData();

    private void LoadData()
    {
        try
        {
            Console.WriteLine("=== ЗАГРУЗКА ДАННЫХ ===");
            Console.WriteLine($"Путь к базе: {_dbPath}");

            if (!File.Exists(_dbPath))
            {
                Console.WriteLine("Файл не найден, создаю новый...");
                Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);

                var defaultData = new
                {
                    users = new[] {
                        new { id = 1, login = "admin", password = "admin", role = "admin" },
                        new { id = 2, login = "student", password = "123", role = "user" }
                    },
                    equipment = new[] {
                        new { id = 1, name = "Камера Sony A7", status = "available", holderId = (int?)null, checkoutDate = (DateTime?)null },
                        new { id = 2, name = "Микрофон Rode NT1", status = "available", holderId = (int?)null, checkoutDate = (DateTime?)null }
                    },
                    history = Array.Empty<HistoryRecord>(),
                    bookings = Array.Empty<RoomBooking>()
                };

                var json = JsonSerializer.Serialize(defaultData, _jsonOptions);
                Console.WriteLine($"Создаю файл с данными");
                File.WriteAllText(_dbPath, json);
                Console.WriteLine("✅ Файл создан!");
            }
            else
            {
                Console.WriteLine("Файл существует, читаю...");
            }

            var fileContent = File.ReadAllText(_dbPath);
            var data = JsonSerializer.Deserialize<JsonElement>(fileContent);

            // 🔑 ВАЖНО: используем _jsonOptions с PropertyNameCaseInsensitive = true
            _users = JsonSerializer.Deserialize<List<User>>(data.GetProperty("users").GetRawText(), _jsonOptions) ?? new List<User>();
            _equipment = JsonSerializer.Deserialize<List<Equipment>>(data.GetProperty("equipment").GetRawText(), _jsonOptions) ?? new List<Equipment>();
            _history = JsonSerializer.Deserialize<List<HistoryRecord>>(data.GetProperty("history").GetRawText(), _jsonOptions) ?? new List<HistoryRecord>();
            _bookings = data.TryGetProperty("bookings", out var bookingsProp) 
                ? JsonSerializer.Deserialize<List<RoomBooking>>(bookingsProp.GetRawText(), _jsonOptions) ?? new List<RoomBooking>()
                : new List<RoomBooking>();

            Console.WriteLine($"✅ Загружено: {_users.Count} пользователей, {_equipment.Count} ед. техники, {_bookings.Count} бронирований");

            // Отладка: покажем, кто загружен
            foreach (var u in _users)
            {
                Console.WriteLine($"  User: {u.Login} / {u.Password} ({u.Role})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка загрузки данных: {ex.Message}");
            // Инициализируем пустыми списками, чтобы приложение не упало
            _users = new List<User>();
            _equipment = new List<Equipment>();
            _history = new List<HistoryRecord>();
        }
    }

    private void SaveData()
    {
        try
        {
            var combined = new { users = _users, equipment = _equipment, history = _history, bookings = _bookings };
            File.WriteAllText(_dbPath, JsonSerializer.Serialize(combined, _jsonOptions));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка сохранения: {ex.Message}");
        }
    }

    public async Task<User?> AuthenticateAsync(string login, string password)
    {
        await Task.Yield();

        Console.WriteLine($"=== АВТОРИЗАЦИЯ ===");
        Console.WriteLine($"Ввод: '{login}' / '{password}'");
        Console.WriteLine($"Пользователей в памяти: {_users.Count}");

        foreach (var user in _users)
        {
            Console.WriteLine($"Проверка: '{user.Login}' / '{user.Password}'");

            // 🔑 Сравнение с учётом регистра (для надёжности)
            if (string.Equals(user.Login, login, StringComparison.OrdinalIgnoreCase) &&
                user.Password == password)
            {
                Console.WriteLine($"✅ УСПЕХ: {user.Login}");
                return user;
            }
        }

        Console.WriteLine("❌ Не найдено");
        return null;
    }

    public async Task<List<Equipment>> GetEquipmentAsync()
    {
        await Task.Yield();
        return _equipment;
    }

    public async Task<bool> CheckoutAsync(int equipmentId, int userId)
    {
        await Task.Yield();
        var eq = _equipment.FirstOrDefault(e => e.Id == equipmentId && e.Status == "available");
        if (eq == null) return false;

        eq.Status = "taken";
        eq.HolderId = userId;
        eq.CheckoutDate = DateTime.Now;
        _history.Add(new HistoryRecord { EquipmentId = eq.Id, UserId = userId, Action = "checkout" });
        SaveData();
        return true;
    }

    public async Task<bool> ReturnAsync(int equipmentId)
    {
        await Task.Yield();
        var eq = _equipment.FirstOrDefault(e => e.Id == equipmentId && e.Status == "taken");
        if (eq == null) return false;

        var previousHolder = eq.HolderId ?? 0;
        eq.Status = "available";
        eq.HolderId = null;
        eq.CheckoutDate = null;
        _history.Add(new HistoryRecord { EquipmentId = eq.Id, UserId = previousHolder, Action = "return" });
        SaveData();
        return true;
    }

    // Оставь старый метод для совместимости
    public async Task<Equipment> AddEquipmentAsync(string name)
    {
        await Task.Yield();
        var newId = _equipment.Any() ? _equipment.Max(e => e.Id) + 1 : 1;
        var eq = new Equipment { Id = newId, Name = name };
        _equipment.Add(eq);
        SaveData();
        return eq;
    }

    // Новый метод с полным объектом
    public async Task<Equipment> AddEquipmentAsync(Equipment equipment)
    {
        await Task.Yield();

        var newId = _equipment.Any() ? _equipment.Max(e => e.Id) + 1 : 1;
        equipment.Id = newId;
        equipment.Status = "available";

        _equipment.Add(equipment);
        SaveData();

        return equipment;
    }

    // Методы для бронирования помещений
    public async Task<List<RoomBooking>> GetRoomBookingsAsync()
    {
        await Task.Yield();
        return _bookings;
    }

    public async Task<bool> BookRoomAsync(RoomBooking booking)
    {
        await Task.Yield();
        
        // Проверка на пересечение времени
        var hasConflict = _bookings.Any(b => 
            b.Date == booking.Date && 
            b.Status != "rejected" &&
            ((booking.StartTime >= b.StartTime && booking.StartTime < b.EndTime) ||
             (booking.EndTime > b.StartTime && booking.EndTime <= b.EndTime) ||
             (booking.StartTime <= b.StartTime && booking.EndTime >= b.EndTime))
        );

        if (hasConflict) return false;

        var newId = _bookings.Any() ? _bookings.Max(b => b.Id) + 1 : 1;
        booking.Id = newId;
        booking.CreatedAt = DateTime.Now;
        booking.Status = "pending";

        _bookings.Add(booking);
        SaveData();
        return true;
    }

    public async Task<bool> ApproveBookingAsync(int bookingId)
    {
        await Task.Yield();
        var booking = _bookings.FirstOrDefault(b => b.Id == bookingId);
        if (booking == null) return false;

        booking.Status = "approved";
        SaveData();
        return true;
    }

    public async Task<bool> RejectBookingAsync(int bookingId)
    {
        await Task.Yield();
        var booking = _bookings.FirstOrDefault(b => b.Id == bookingId);
        if (booking == null) return false;

        booking.Status = "rejected";
        SaveData();
        return true;
    }
}