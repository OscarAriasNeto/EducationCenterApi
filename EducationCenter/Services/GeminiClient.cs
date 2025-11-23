using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace EducationCenter.Services;

public class GeminiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        _apiKey = configuration["Gemini:ApiKey"]
                  ?? throw new InvalidOperationException("Gemini:ApiKey não configurado no appsettings.json.");

        _model = configuration["Gemini:Model"] ?? "gemini-1.5-flash";
    }

    public async Task<string> AskAsync(string prompt)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        using var response = await _httpClient.PostAsJsonAsync(url, payload);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        var text = doc
            .RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return text ?? string.Empty;
    }
}
