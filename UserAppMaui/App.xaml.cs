using System;

namespace UserAppMaui;

public partial class App : Application
{
    public static IServiceProvider Services { get; internal set; } = default!;

    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }
}
