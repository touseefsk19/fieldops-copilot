using FieldOps.Mobile.Services;
using FieldOps.Mobile.ViewModels;
using FieldOps.Mobile.Views;
using Microsoft.Extensions.Logging;

namespace FieldOps.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if ANDROID
        const string baseUrl = "http://10.0.2.2:5266";   // Android emulator's name for your Mac
#else
        const string baseUrl = "http://localhost:5266";
#endif

        builder.Services.AddSingleton<TokenStore>();
        builder.Services.AddHttpClient<FieldOpsApi>(c =>
        {
            c.BaseAddress = new Uri(baseUrl);
            c.Timeout = TimeSpan.FromMinutes(2);          // local LLM answers can be slow
        });

        builder.Services.AddTransient<AskViewModel>();
        builder.Services.AddTransient<AskPage>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}