using Microsoft.Extensions.DependencyInjection;
using UserAppMaui.ViewModels;

namespace UserAppMaui.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
        var vm = App.Services.GetRequiredService<RegisterViewModel>();
        BindingContext = vm;
    }

    public RegisterPage(RegisterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
