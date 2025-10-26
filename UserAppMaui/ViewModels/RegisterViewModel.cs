using System.Text.RegularExpressions;
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
        CanRegister = true;
    }

    string firstName = string.Empty;
    string lastName = string.Empty;
    string email = string.Empty;
    string password = string.Empty;
    string confirm = string.Empty;

    string? firstNameError;
    string? lastNameError;
    string? emailError;
    string? passwordError;
    string? confirmError;

    bool canRegister;
    bool isValid;

    public string FirstName
    {
        get => firstName;
        set { SetProperty(ref firstName, value); Validate(false); }
    }

    public string LastName
    {
        get => lastName;
        set { SetProperty(ref lastName, value); Validate(false); }
    }

    public string Email
    {
        get => email;
        set { SetProperty(ref email, value); Validate(false); }
    }

    public string Password
    {
        get => password;
        set { SetProperty(ref password, value); Validate(false); }
    }

    public string Confirm
    {
        get => confirm;
        set { SetProperty(ref confirm, value); Validate(false); }
    }

    public string? FirstNameError
    {
        get => firstNameError;
        set => SetProperty(ref firstNameError, value);
    }

    public string? LastNameError
    {
        get => lastNameError;
        set => SetProperty(ref lastNameError, value);
    }

    public string? EmailError
    {
        get => emailError;
        set => SetProperty(ref emailError, value);
    }

    public string? PasswordError
    {
        get => passwordError;
        set => SetProperty(ref passwordError, value);
    }

    public string? ConfirmError
    {
        get => confirmError;
        set => SetProperty(ref confirmError, value);
    }

    public bool CanRegister
    {
        get => canRegister;
        private set { SetProperty(ref canRegister, value); RegisterCommand.RaiseCanExecuteChanged(); }
    }

    public bool IsValid
    {
        get => isValid;
        private set => SetProperty(ref isValid, value);
    }

    public AsyncCommand RegisterCommand { get; }

    void Validate(bool showErrors)
    {
        string? fnErr = string.IsNullOrWhiteSpace(FirstName) ? "Podaj imię." : null;
        string? lnErr = string.IsNullOrWhiteSpace(LastName) ? "Podaj nazwisko." : null;

        string? emErr = string.IsNullOrWhiteSpace(Email)
            ? "Podaj e-mail."
            : (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") ? "Nieprawidłowy e-mail." : null);

        string? pwErr;
        if (string.IsNullOrWhiteSpace(Password)) pwErr = "Podaj hasło.";
        else if (Password.Length < 8) pwErr = "Hasło min. 8 znaków.";
        else if (!Regex.IsMatch(Password, @"(?=.*[A-Za-z])(?=.*\d)")) pwErr = "Hasło musi mieć literę i cyfrę.";
        else pwErr = null;

        string? cfErr = string.IsNullOrWhiteSpace(Confirm) ? "Powtórz hasło."
                      : (Confirm != Password ? "Hasła nie są zgodne." : null);

        IsValid = fnErr is null && lnErr is null && emErr is null && pwErr is null && cfErr is null;

        if (showErrors)
        {
            FirstNameError = fnErr;
            LastNameError = lnErr;
            EmailError = emErr;
            PasswordError = pwErr;
            ConfirmError = cfErr;
        }
    }

    async Task RegisterAsync()
    {
        Validate(true);
        if (!IsValid) return;

        try
        {
            IsBusy = true;
            RegisterCommand.RaiseCanExecuteChanged();

            var (ok, message) = await _api.Register(
                Email.Trim(),
                Password,
                Confirm,
                FirstName.Trim(),
                LastName.Trim());

            if (ok)
            {
                await Shell.Current.DisplayAlert("Sukces", "Konto utworzone. Zaloguj się.", "OK");
                await Shell.Current.GoToAsync("//login");
            }
            else
            {
                Error = message ?? "Rejestracja nieudana.";
            }
        }
        catch (Exception ex)
        {
            Error = $"Błąd: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            RegisterCommand.RaiseCanExecuteChanged();
        }
    }
}
