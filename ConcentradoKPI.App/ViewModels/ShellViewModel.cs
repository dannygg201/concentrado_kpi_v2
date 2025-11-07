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
                if (_currentView == value) return;
                _currentView = value;
                OnPropertyChanged();
                NavigateRequested?.Invoke(_currentView);   // avisa al ShellWindow para cargar la vista
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
                {
                    CurrentView = v;           // 🔹 Actualiza la propiedad
                    NavigateRequested?.Invoke(v); // 🔹 Avisa al ShellWindow para que cargue la vista
                }
            });
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
