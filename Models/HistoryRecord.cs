using System;  // ← ДОБАВИТЬ ЭТУ СТРОКУ!

namespace MediaApp2.Models;

public class HistoryRecord
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public int UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}