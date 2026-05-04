using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaApp2.Models;

public class RoomBooking
{
    public int Id { get; set; }
    public int? UserId { get; set; } // Nullable для гостей
    public string UserName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty; // Email для гостей
    public string GuestPhone { get; set; } = string.Empty; // Телефон для гостей
    public bool IsGuest { get; set; } = false; // Флаг: гость или пользователь
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string Status { get; set; } = "pending"; // pending, approved, rejected, completed
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Статус дня: Free, PartiallyBusy, FullyBusy
    /// </summary>
    public DayStatus GetDayStatus()
    {
        var totalHours = (EndTime - StartTime).TotalHours;
        if (totalHours >= 8) return DayStatus.FullyBusy;
        if (totalHours >= 4) return DayStatus.PartiallyBusy;
        return DayStatus.Free;
    }
}

public enum DayStatus
{
    Free,           // Свободно
    PartiallyBusy,  // Частично занято
    FullyBusy       // Занято полностью
}
