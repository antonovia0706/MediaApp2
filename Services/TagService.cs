using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaApp2.Services;

public class TagService
{
    // Предопределённые теги с цветами
    private static readonly Dictionary<string, string> _defaultTags = new()
    {
        { "Камера", "#e74c3c" },
        { "Зеркальная", "#3498db" },
        { "Беззеркальная", "#9b59b6" },
        { "Объектив", "#2ecc71" },
        { "Микрофон", "#f39c12" },
        { "Штатив", "#1abc9c" },
        { "Свет", "#e67e22" },
        { "4K", "#8e44ad" },
        { "Для интервью", "#2980b9" },
        { "Для съёмки", "#27ae60" },
        { "Аренда", "#d35400" },
        { "Студийная", "#c0392b" },
        { "Портативная", "#16a085" },
        { "Bluetooth", "#7f8c8d" },
        { "USB", "#95a5a6" }
    };

    public List<string> GetAvailableTags() => _defaultTags.Keys.ToList();

    public string GetTagColor(string tag) =>
        _defaultTags.TryGetValue(tag, out var color) ? color : "#95a5a6";

    public void AddTag(string tag, string color)
    {
        if (!_defaultTags.ContainsKey(tag))
            _defaultTags[tag] = color;
    }

    public Dictionary<string, string> GetAllTags() => new(_defaultTags);
}