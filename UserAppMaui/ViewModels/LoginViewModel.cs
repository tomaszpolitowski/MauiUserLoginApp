using UserAppMaui.Commands;
using UserAppMaui.Services;

namespace UserAppMaui.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly ApiClient _api;

    public LoginViewModel(ApiClient api)
    {
        _api = api;
        LoginCommand = new AsyncCommand(LoginAsync, () => !IsBusy);
        GoRegisterCommand = new AsyncCommand(() => Shell.Current.GoToAsync("//register"));
    }

    string email = "";
    public string Email { get => email; set => SetProperty(ref email, value); }

    string password = "";
    public string Password { get => password; set => SetProperty(ref password, value); }

    public AsyncCommand LoginCommand { get; }
    public AsyncCommand GoRegisterCommand { get; }

    private async Task LoginAsync()
    {
        Error = null; IsBusy = true; LoginCommand.RaiseCanExecuteChanged();
        try
        {
            var (ok, message) = await _api.Login(Email.Trim(), Password);
            if (ok) await Shell.Current.GoToAsync("//profile");
            else Error = message ?? "Logowanie nieudane.";
        }
        finally { IsBusy = false; LoginCommand.RaiseCanExecuteChanged(); }
    }
}
