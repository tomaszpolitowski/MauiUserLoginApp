using UserAppMaui.Commands;
using UserAppMaui.Services;

namespace UserAppMaui.ViewModels;

public class RegisterViewModel : BaseViewModel
{
    private readonly ApiClient _api;

    public RegisterViewModel(ApiClient api)
    {
        _api = api;
        RegisterCommand = new AsyncCommand(RegisterAsync, () => !IsBusy);
    }

    public string FirstName { get => firstName; set => SetProperty(ref firstName, value); }
    public string LastName { get => lastName; set => SetProperty(ref lastName, value); }
    public string Email { get => email; set => SetProperty(ref email, value); }
    public string Password { get => password; set => SetProperty(ref password, value); }
    public string Confirm { get => confirm; set => SetProperty(ref confirm, value); }

    string firstName = "", lastName = "", email = "", password = "", confirm = "";

    public AsyncCommand RegisterCommand { get; }

    private async Task RegisterAsync()
    {
        Error = null; IsBusy = true; RegisterCommand.RaiseCanExecuteChanged();
        try
        {
            var (ok, message) = await _api.Register(Email.Trim(), Password, Confirm, FirstName.Trim(), LastName.Trim());
            if (ok)
            {
                await Application.Current.MainPage.DisplayAlert("Sukces", "Konto utworzone. Zaloguj się.", "OK");
                await Shell.Current.GoToAsync("//login");
            }
            else Error = message ?? "Rejestracja nieudana.";
        }
        finally { IsBusy = false; RegisterCommand.RaiseCanExecuteChanged(); }
    }
}
