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
            // 0) Liberar la vista/VM anterior si implementa IDisposable
            if (BodyHost.Content is FrameworkElement oldView && oldView.DataContext is IDisposable oldVm)
                oldVm.Dispose();

            switch (v)
            {
                case AppView.PersonalVigente:
                    {
                        var vm = new PersonalVigenteViewModel(_company, _project, _week);
                        var view = new PersonalVigenteView { DataContext = vm };
                        BodyHost.Content = view;
                        TopBarControl.Title = "Personal vigente";
                        // OJO: evita NotifyAll() si te generaba reentradas
                        // _week.Live.NotifyAll();
                        CommandManager.InvalidateRequerySuggested();
                        break;
                    }
                case AppView.PiramideSeguridad:
                    {
                        var vm = new PiramideSeguridadViewModel(_company, _project, _week);
                        var view = new PiramideSeguridadView { DataContext = vm };
                        BodyHost.Content = view;
                        TopBarControl.Title = "Pirámide de seguridad";
                        // _week.Live.NotifyAll();
                        CommandManager.InvalidateRequerySuggested();
                        break;
                    }
                case AppView.InformeSemanalCma:
                    {
                        var vm = new InformeSemanalCMAViewModel(_company, _project, _week);
                        var view = new InformeSemanalCmaView { DataContext = vm };
                        BodyHost.Content = view;
                        TopBarControl.Title = "Informe semanal CMA";
                        // _week.Live.NotifyAll();
                        CommandManager.InvalidateRequerySuggested();
                        break;
                    }
                case AppView.PrecursorSif:
                    {
                        var vm = new PrecursorSifViewModel(_company, _project, _week);
                        var view = new PrecursorSifView { DataContext = vm };
                        BodyHost.Content = view;
                        TopBarControl.Title = "Precursor SIF";
                        // _week.Live.NotifyAll();
                        CommandManager.InvalidateRequerySuggested();
                        break;
                    }
                case AppView.Incidentes:
                    {
                        var vm = new IncidentesViewModel(_company, _project, _week);
                        var view = new IncidentesView { DataContext = vm };
                        BodyHost.Content = view;
                        TopBarControl.Title = "Incidentes";
                        // _week.Live.NotifyAll();
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
        protected override void OnClosed(EventArgs e)
        {
            // Intenta disponer la vista actual si su VM implementa IDisposable
            if (BodyHost?.Content is FrameworkElement fe && fe.DataContext is IDisposable disp)
                disp.Dispose();

            base.OnClosed(e);
        }

    }
}
