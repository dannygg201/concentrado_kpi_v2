// ViewModels/ShellViewModel.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ConcentradoKPI.App.Models;

namespace ConcentradoKPI.App.ViewModels
{
    public class ShellViewModel : INotifyPropertyChanged
    {
        private AppView _currentView = AppView.PersonalVigente;
        public AppView CurrentView
        {
            get => _currentView;
            set
            {
                if (_currentView != value)
                {
                    _currentView = value;
                    OnPropertyChanged();
                }
            }
        }

        // La NavBar disparará esto y la ventana decide a dónde navegar
        public event Action<AppView>? NavigateRequested;

        // Usa TU RelayCommand no genérico (el que ya tienes en el proyecto)
        public ICommand NavigateCommand { get; }

        public ShellViewModel()
        {
            NavigateCommand = new RelayCommand(param =>
            {
                if (param is AppView v)
                    NavigateRequested?.Invoke(v);
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
