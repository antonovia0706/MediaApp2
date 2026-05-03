using MediaApp2.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MediaApp2.Services;

public class VkService
{
    private readonly string _groupId;
    private readonly string _accessToken;
    private readonly HttpClient _httpClient;

    public VkService(string groupId, string accessToken = "")
    {
        _groupId = groupId;
        _accessToken = accessToken;
        _httpClient = new HttpClient();
    }

    public async Task<List<VkPost>> GetWallPostsAsync(int count = 10)
    {
        try
        {
            // Если нет токена, используем тестовые данные
            if (string.IsNullOrEmpty(_accessToken))
            {
                return GetTestPosts();
            }

            var url = $"https://api.vk.com/method/wall.get?owner_id={_groupId}&count={count}&access_token={_accessToken}&v=5.199";

            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);
            var posts = new List<VkPost>();

            if (json.RootElement.TryGetProperty("response", out var responseElement) &&
                responseElement.TryGetProperty("items", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    posts.Add(new VkPost
                    {
                        Id = item.GetProperty("id").GetInt32(),
                        Text = item.TryGetProperty("text", out var text) ? text.GetString() : "",
                        Date = DateTimeOffset.FromUnixTimeSeconds(item.GetProperty("date").GetInt64()).DateTime,
                        LikesCount = item.TryGetProperty("likes", out var likes)
                            ? likes.GetProperty("count").GetInt32()
                            : 0,
                        AuthorName = "Группа ВК"
                    });
                }
            }

            return posts;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении постов: {ex.Message}");
            return GetTestPosts(); // Возвращаем тестовые данные при ошибке
        }
    }

    private List<VkPost> GetTestPosts()
    {
        return new List<VkPost>
        {
            new VkPost
            {
                Id = 1,
                Text = "📢 Внимание! Открыта запись на съёмки в новом учебном году. Успейте занять место!",
                Date = DateTime.Now.AddHours(-2),
                AuthorName = "Медиацентр",
                LikesCount = 15
            },
            new VkPost
            {
                Id = 2,
                Text = "🎥 Новое оборудование: теперь у нас есть Sony A7 IV и микрофоны Rode!",
                Date = DateTime.Now.AddDays(-1),
                AuthorName = "Медиацентр",
                LikesCount = 28
            },
            new VkPost
            {
                Id = 3,
                Text = "📸 Мастер-класс по фотографии пройдёт в эту субботу в 14:00. Ждём всех желающих!",
                Date = DateTime.Now.AddDays(-2),
                AuthorName = "Медиацентр",
                LikesCount = 42
            }
        };
    }
}