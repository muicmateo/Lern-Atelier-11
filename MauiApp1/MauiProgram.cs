using CommunityToolkit.Maui;
using MauiApp1.Models;
using MauiApp1.Services;
using MauiApp1.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection; // Add this using directive
using System.Net.Http; // Optional, but sometimes needed for HttpClient
using Microsoft.Extensions.Http; // <-- Add this using directive

namespace MauiApp1;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        }).UseMauiCommunityToolkit();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Configure settings
        ConfigureSettings(builder);

        // Register services
        builder.Services.AddHttpClient<IGeminiService, GeminiService>();
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();

        return builder.Build();
    }

    private static void ConfigureSettings(MauiAppBuilder builder)
    {
        try
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            using var stream = executingAssembly.GetManifestResourceStream("MauiApp1.appsettings.json");

            if (stream != null)
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonStream(stream)
                    .Build();

                builder.Configuration.AddConfiguration(configuration);
            }
            else
            {
                // Fallback - create empty configuration section
                var configDict = new Dictionary<string, string>
                {
                    { "GeminiSettings:ApiKey", "" }
                };

                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(configDict)
                    .Build();

                builder.Configuration.AddConfiguration(configuration);
            }

            // Add user secrets for development
            builder.Configuration.AddUserSecrets<App>();

            builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));
        }
        catch (Exception ex)
        {
            // Log the error or handle appropriately
            System.Diagnostics.Debug.WriteLine($"Configuration error: {ex.Message}");

            // Provide fallback configuration
            var configDict = new Dictionary<string, string>
            {
                { "GeminiSettings:ApiKey", "" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            builder.Configuration.AddConfiguration(configuration);
            builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));
        }
    }
}

