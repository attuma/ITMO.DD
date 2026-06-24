using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace itmodd.ViewModels;

public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    // кричит экрану "свойство изменилось". имя свойства подставится само
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}