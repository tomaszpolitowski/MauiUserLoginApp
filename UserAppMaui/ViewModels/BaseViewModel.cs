using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UserAppMaui.ViewModels;

public abstract class BaseViewModel : INotifyPropertyChanged
{
    bool isBusy;
    string? error;

    public bool IsBusy { get => isBusy; set => SetProperty(ref isBusy, value); }
    public string? Error { get => error; set => SetProperty(ref error, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T backing, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(backing, value)) return false;
        backing = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }
}
