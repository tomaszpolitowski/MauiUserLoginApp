﻿namespace UserAppMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("login", typeof(Views.LoginPage));
        Routing.RegisterRoute("register", typeof(Views.RegisterPage));
        Routing.RegisterRoute("profile", typeof(Views.ProfilePage));
    }
}
