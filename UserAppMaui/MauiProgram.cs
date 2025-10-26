using Microsoft.Extensions.Logging;
using UserAppMaui.Services;
using UserAppMaui.ViewModels;
using UserAppMaui.Views;

namespace UserAppMaui;

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

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Windows
        const string ApiBaseUrl = "https://localhost:7067/";
        // const string ApiBaseUrl = "https://10.0.2.2:7067/";

        // Services
        builder.Services.AddSingleton(new ApiClient(ApiBaseUrl));

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();

        // Views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ProfilePage>();

        var app = builder.Build();
        App.Services = app.Services;

        return app;
    }
}
