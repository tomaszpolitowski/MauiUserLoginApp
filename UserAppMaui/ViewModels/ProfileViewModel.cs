using UserAppMaui.Commands;
using UserAppMaui.Services;

namespace UserAppMaui.ViewModels;

public class ProfileViewModel : BaseViewModel
{
    private readonly ApiClient _api;

    public ProfileViewModel(ApiClient api)
    {
        _api = api;
        RefreshCommand = new AsyncCommand(LoadAsync);
        LogoutCommand = new AsyncCommand(LogoutAsync);
    }

    string fullName = "", email = "", createdAt = "";
    public string FullName { get => fullName; set => SetProperty(ref fullName, value); }
    public string Email { get => email; set => SetProperty(ref email, value); }
    public string CreatedAt { get => createdAt; set => SetProperty(ref createdAt, value); }

    public AsyncCommand RefreshCommand { get; }
    public AsyncCommand LogoutCommand { get; }

    public async Task LoadAsync()
    {
        Error = null; IsBusy = true;
        try
        {
            var (user, message) = await _api.Me();
            if (user == null) { Error = message ?? "Nie udało się pobrać danych."; return; }
            FullName = $"{user.FirstName} {user.LastName}";
            Email = user.Email;
            CreatedAt = $"Utworzono: {user.CreatedAt}";
        }
        finally { IsBusy = false; }
    }

    private async Task LogoutAsync()
    {
        await _api.Logout();
        await Shell.Current.GoToAsync("//login");
    }
}
