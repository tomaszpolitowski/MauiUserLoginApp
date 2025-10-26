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

    string fullName = string.Empty;
    string email = string.Empty;
    string createdAt = string.Empty;

    public string FullName
    {
        get => fullName;
        set => SetProperty(ref fullName, value);
    }

    public string Email
    {
        get => email;
        set => SetProperty(ref email, value);
    }

    public string CreatedAt
    {
        get => createdAt;
        set => SetProperty(ref createdAt, value);
    }

    public AsyncCommand RefreshCommand { get; }
    public AsyncCommand LogoutCommand { get; }

    public async Task LoadAsync()
    {
        Error = null;
        IsBusy = true;

        try
        {
            var (user, msg) = await _api.Me();

            if (user == null)
            {
                // 401 → przenieś na ekran logowania
                if (msg != null && msg.Contains("401", StringComparison.Ordinal))
                {
                    await Shell.Current.GoToAsync("//login");
                    return;
                }

                Error = msg ?? "Nie udało się pobrać danych.";
                return;
            }

            FullName = $"{user.FirstName} {user.LastName}";
            Email = user.Email;

            if (DateTimeOffset.TryParse(user.CreatedAt, out var dto))
                CreatedAt = $"Utworzono: {dto:dd.MM.yyyy}";
            else
                CreatedAt = $"Utworzono: {user.CreatedAt}";

        }
        catch (Exception ex)
        {
            Error = $"Błąd: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LogoutAsync()
    {
        await _api.Logout();
        await Shell.Current.GoToAsync("//login");
    }
}
