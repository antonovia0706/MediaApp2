using System;
using System.Collections.Generic;
using System.Linq;  // ← ВАЖНО!

namespace MediaApp2.Models;

public class Equipment
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }
    public string Status { get; set; } = "available";
    public int? HolderId { get; set; }
    public DateTime? CheckoutDate { get; set; }

    // Вспомогательное свойство для отображения тегов
    public List<string> TagList =>
        string.IsNullOrWhiteSpace(Tags)
            ? new List<string>()
            : Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    // Свойство для отображения состояния корзины
    [System.ComponentModel.DataAnnotations.NotMapped]
    public bool IsInCart { get; set; } = false;
}