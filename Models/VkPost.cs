using System;

namespace MediaApp2.Models;

public class VkPost
{
    public int Id { get; set; }
    public string? Text { get; set; }
    public DateTime Date { get; set; }
    public string? AuthorName { get; set; }
    public int LikesCount { get; set; }
    public string? PhotoUrl { get; set; }
}