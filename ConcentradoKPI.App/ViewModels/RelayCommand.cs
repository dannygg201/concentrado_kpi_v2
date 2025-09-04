using System;
using System.Windows.Input;

namespace ConcentradoKPI.App.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _exec;
        private readonly Func<object?, bool>? _can;

        public RelayCommand(Action<object?> exec, Func<object?, bool>? can = null)
        {
            _exec = exec;
            _can = can;
        }

        public bool CanExecute(object? parameter) => _can?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _exec(parameter);

        // MUY IMPORTANTE: enganchamos al CommandManager para que WPF re-evalue automáticamente
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged()
        {
            // Forzamos re-evaluación de todos los comandos
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
