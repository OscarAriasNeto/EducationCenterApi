using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace EducationCenter.Services;

public class GeminiClient
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;

    public GeminiClient(HttpClient httpClient, IOptions<GeminiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> AskAsync(string prompt)
    {
        var url =
            $"https://generativelanguage.googleapis.com/v1/models/{_options.Model}:generateContent?key={_options.ApiKey}";


        var requestBody = new
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

        var response = await _httpClient.PostAsJsonAsync(url, requestBody);

        // SE der erro (404, 401, etc) não joga exceção: devolve uma mensagem explicando
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            return $"Erro ao chamar Gemini: {(int)response.StatusCode} - {response.ReasonPhrase}. Detalhes: {errorBody}";
        }

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = doc.RootElement;

        var text = root
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;

        return text;
    }
}