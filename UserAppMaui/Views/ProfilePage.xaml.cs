using Microsoft.Extensions.DependencyInjection;
using UserAppMaui.ViewModels;

namespace UserAppMaui.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
        var vm = App.Services.GetRequiredService<ProfileViewModel>();
        BindingContext = vm;
    }

    public ProfilePage(ProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ProfileViewModel vm)
            await vm.LoadAsync();
    }
}
