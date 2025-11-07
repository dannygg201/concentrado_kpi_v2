using System;
using System.Windows;
using System.Windows.Input;
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.ViewModels;
using ConcentradoKPI.App.Views.Pages;
using ConcentradoKPI.App.Views.Controls;
using ConcentradoKPI.App.Views.Abstractions;
using ConcentradoKPI.App.Services;

namespace ConcentradoKPI.App.Views
{
    public partial class ShellWindow : Window
    {
        private readonly Company _company;
        private readonly Project _project;
        private readonly WeekData _week;
        private readonly MainViewModel _hostVm;

        private readonly ShellViewModel _shellVm = new();

        public ShellWindow(Company c, Project p, WeekData w, MainViewModel hostVm)
        {
            InitializeComponent();

            TopBarControl.CommandTarget = this;

            _company = c; _project = p; _week = w; _hostVm = hostVm;

            // 1) DC del Window -> ShellViewModel (NavBar)
            DataContext = _shellVm;

            // 2) DC del TopBar -> contexto con Company/Project/Week
            TopBarControl.DataContext = new TopBarContext(_company, _project, _week);

            // 3) Navegación
            _shellVm.NavigateRequested += v => LoadView(v);

            // 4) Vista inicial
            _shellVm.CurrentView = AppView.PersonalVigente;
            LoadView(_shellVm.CurrentView);
        }

        private void LoadView(AppView v)
        {
            switch (v)
            {
                case AppView.PersonalVigente:
                    {
                        var view = new PersonalVigenteView
                        {
                            DataContext = new PersonalVigenteViewModel(_company, _project, _week)
                        };
                        BodyHost.Content = view;
                        TopBarControl.Title = "Personal vigente";
                        // 🔔 Asegura que cualquier VM que escuche a Live arranque con estado actual
                        _week.Live.NotifyAll();
                        CommandManager.InvalidateRequerySuggested();
                        break;
                    }
                case AppView.PiramideSeguridad:
                    {
                        var view = new PiramideSeguridadView
                        {
                            DataContext = new PiramideSeguridadViewModel(_company, _project, _week)
                        };
                        BodyHost.Content = view;
                        TopBarControl.Title = "Pirámide de seguridad";
                        // 🔔 Dispara que el VM de pirámide lea del Live al instante
                        _week.Live.NotifyAll();
                        CommandManager.InvalidateRequerySuggested();
                        break;
                    }
                case AppView.InformeSemanalCma:
                    {
                        var view = new InformeSemanalCmaView
                        {
                            DataContext = new InformeSemanalCMAViewModel(_company, _project, _week)
                        };
                        BodyHost.Content = view;
                        TopBarControl.Title = "Informe semanal CMA";
                        _week.Live.NotifyAll();
                        CommandManager.InvalidateRequerySuggested();
                        break;
                    }
                case AppView.PrecursorSif:
                    {
                        var view = new PrecursorSifView
                        {
                            DataContext = new PrecursorSifViewModel(_company, _project, _week)
                        };
                        BodyHost.Content = view;
                        TopBarControl.Title = "Precursor SIF";
                        _week.Live.NotifyAll();
                        CommandManager.InvalidateRequerySuggested();
                        break;
                    }
                case AppView.Incidentes:
                    {
                        var view = new IncidentesView
                        {
                            DataContext = new IncidentesViewModel(_company, _project, _week)
                        };
                        BodyHost.Content = view;
                        TopBarControl.Title = "Incidentes";
                        _week.Live.NotifyAll();
                        CommandManager.InvalidateRequerySuggested();
                        break;
                    }
            }
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _company != null && _project != null && _week != null;
        }

        private async void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // 1) UI -> WeekData (si la vista lo implementa)
            if (BodyHost.Content is ISyncToWeek sync)
                sync.SyncIntoWeek();

            // 2) Persistir
            try
            {
                var path = await ProjectStorageService.SaveOrPromptAsync(this);
                MessageBox.Show(this, $"Guardado correctamente.\n\nArchivo:\n{path}",
                                "Guardar", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"No se pudo guardar.\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
