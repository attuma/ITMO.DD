using System.Windows.Input;

namespace itmodd.Commands;

public class RelayCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => throw new NotImplementedException();

    public void Execute(object? parameter) => throw new NotImplementedException();
}
