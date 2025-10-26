using Microsoft.Extensions.DependencyInjection;
using UserAppMaui.ViewModels;

namespace UserAppMaui.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        var vm = App.Services.GetRequiredService<LoginViewModel>();
        BindingContext = vm;
    }

    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
