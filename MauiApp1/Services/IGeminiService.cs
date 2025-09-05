namespace MauiApp1.Services;

public interface IGeminiService
{
    Task<string> GetCompletionAsync(string prompt);
    Task<string> GenerateStoryAsync(string prompt);
}