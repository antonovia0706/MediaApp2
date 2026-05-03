using System;

namespace MediaApp2.Models;

public class RoomBooking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string Status { get; set; } = "pending"; // pending, approved, rejected, completed
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
