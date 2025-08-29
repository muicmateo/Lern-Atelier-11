using MauiApp1.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MauiApp1.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;
    private readonly string _endpoint;

    public GeminiService(HttpClient httpClient, IOptions<GeminiSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-pro:generateContent?key={_settings.ApiKey}";
    }

    public async Task<string> GetCompletionAsync(string prompt)
    {
        var payload = new GeminiRequest([new Content([new Part(prompt)])]);
        
        try
        {
            var response = await _httpClient.PostAsJsonAsync(_endpoint, payload);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return $"Error: {response.StatusCode} - {errorContent}";
            }

            var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            return geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "No response from model.";
        }
        catch (Exception ex)
        {
            return $"An exception occurred: {ex.Message}";
        }
    }
}

// Request and Response Models
public record Part(string Text);
public record Content(List<Part> Parts);
public record GeminiRequest(List<Content> Contents);

public record Candidate
{
    [JsonPropertyName("content")]
    public Content Content { get; init; }
}

public record GeminiResponse
{
    [JsonPropertyName("candidates")]
    public List<Candidate> Candidates { get; init; }
}