using System;
using System.ComponentModel;
using System.Linq;
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

            // El TopBar enviará ApplicationCommands.Save a esta ventana
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

            // Confirmar guardado al cerrar si hay cambios
            this.Closing += OnShellClosing_ConfirmSave;
        }

        private void LoadView(AppView v)
        {
            // Liberar vista/VM anterior si implementa IDisposable
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
                        CommandManager.InvalidateRequerySuggested();
                        break;
                    }
                case AppView.PiramideSeguridad:
                    {
                        var vm = new PiramideSeguridadViewModel(_company, _project, _week);
                        var view = new PiramideSeguridadView { DataContext = vm };
                        BodyHost.Content = view;
                        TopBarControl.Title = "Pirámide de seguridad";
                        CommandManager.InvalidateRequerySuggested();
                        break;
                    }
                case AppView.InformeSemanalCma:
                    {
                        var vm = new InformeSemanalCMAViewModel(_company, _project, _week);
                        var view = new InformeSemanalCmaView { DataContext = vm };
                        BodyHost.Content = view;
                        TopBarControl.Title = "Informe semanal CMA";
                        CommandManager.InvalidateRequerySuggested();
                        break;
                    }
                case AppView.PrecursorSif:
                    {
                        var vm = new PrecursorSifViewModel(_company, _project, _week);
                        var view = new PrecursorSifView { DataContext = vm };
                        BodyHost.Content = view;
                        TopBarControl.Title = "Precursor SIF";
                        CommandManager.InvalidateRequerySuggested();
                        break;
                    }
                case AppView.Incidentes:
                    {
                        var vm = new IncidentesViewModel(_company, _project, _week);
                        var view = new IncidentesView { DataContext = vm };
                        BodyHost.Content = view;
                        TopBarControl.Title = "Incidentes";
                        CommandManager.InvalidateRequerySuggested();
                        break;
                    }
            }
        }

        // Habilitación del botón Guardar (TopBar)
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Habilita si existe un proyecto cargado (y opcionalmente si hay cambios)
            e.CanExecute = ProjectStorageService.CurrentData != null;
            // e.CanExecute = ProjectStorageService.CurrentData != null && ProjectStorageService.IsDirty;
        }

        // Sin depender de MainViewModel.AppData: reflejamos _week en el AppData actual
        private void MirrorWeekIntoCurrentRoot()
        {
            var app = ProjectStorageService.CurrentData;
            if (app == null || app.Companies == null) return;

            var comp = app.Companies.FirstOrDefault(x => string.Equals(x.Name, _company.Name, StringComparison.Ordinal));
            if (comp == null || comp.Projects == null) return;

            var proj = comp.Projects.FirstOrDefault(x => string.Equals(x.Name, _project.Name, StringComparison.Ordinal));
            if (proj == null || proj.Weeks == null) return;

            var wk = proj.Weeks.FirstOrDefault(x => x.WeekNumber == _week.WeekNumber);
            if (wk == null) return;

            // Copiamos los documentos que pudo haber modificado el Shell
            // (agrega aquí otros que uses dentro de WeekData)
            if (_week.PersonalVigente != null) wk.PersonalVigente = _week.PersonalVigente;
            if (_week.PrecursorSif != null) wk.PrecursorSif = _week.PrecursorSif;
            if (_week.Incidentes != null) wk.Incidentes = _week.Incidentes;

            // Si hay más propiedades que modifiques en otros módulos, repítelo aquí.
        }

        // Ejecución del Guardar
        private async void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // 1) UI -> WeekData (si la vista lo implementa)
            if (BodyHost.Content is ISyncToWeek sync)
                sync.SyncIntoWeek();

            // 2) Reflejar esos cambios en el AppData actual (aunque sean instancias distintas)
            MirrorWeekIntoCurrentRoot();

            // 3) Persistir
            try
            {
                var ok = await ProjectStorageService.SaveOrPromptAsync(this);
                if (!ok) return; // usuario canceló “Guardar como…”
                // (Opcional) toast/confirmación
                // MessageBox.Show(this, "Guardado correctamente.", "Guardar", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (OperationCanceledException) { /* cancelado por el usuario */ }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"No se pudo guardar.\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Confirmar guardado al cerrar
        private async void OnShellClosing_ConfirmSave(object? sender, CancelEventArgs e)
        {
            if (!ProjectStorageService.IsDirty) return;

            var r = MessageBox.Show(this,
                "Hay cambios sin guardar.\n\n¿Deseas guardarlos antes de salir?",
                "Cambios sin guardar",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (r == MessageBoxResult.Cancel) { e.Cancel = true; return; }
            if (r == MessageBoxResult.Yes)
            {
                try
                {
                    // Por si había cambios sin volcar
                    if (BodyHost.Content is ISyncToWeek sync)
                        sync.SyncIntoWeek();

                    MirrorWeekIntoCurrentRoot();

                    var ok = await ProjectStorageService.SaveOrPromptAsync(this);
                    if (!ok) e.Cancel = true; // canceló diálogo
                }
                catch (OperationCanceledException)
                {
                    e.Cancel = true;
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Disponer VM actual si aplica
            if (BodyHost?.Content is FrameworkElement fe && fe.DataContext is IDisposable disp)
                disp.Dispose();

            base.OnClosed(e);
        }
    }
}
