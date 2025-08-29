using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Services;

namespace MauiApp1.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IGeminiService _geminiService;

    [ObservableProperty]
    private string _prompt = string.Empty;

    [ObservableProperty]
    private string _response = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public MainViewModel(IGeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    public bool IsGeminiEnabled => !IsBusy && !string.IsNullOrWhiteSpace(Prompt);

    [RelayCommand]
    private async Task AskGemini()
    {
        if (string.IsNullOrWhiteSpace(Prompt))
            return;

        IsBusy = true;
        Response = "Thinking...";
        
        try
        {
            Response = await _geminiService.GetCompletionAsync(Prompt);
        }
        catch (Exception ex)
        {
            Response = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnPromptChanged(string value)
    {
        OnPropertyChanged(nameof(IsGeminiEnabled));
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsGeminiEnabled));
    }
}