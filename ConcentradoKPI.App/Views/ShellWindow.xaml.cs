using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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

            // El TopBar enviará ApplicationCommands.Save / Close a esta ventana
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

            // 5) Confirmar guardado al cerrar si hay cambios (X de la ventana)
            this.Closing += OnShellClosing_ConfirmSave;

            // 6) Comando de Cerrar (para botón “Salir” del TopBar)
            CommandBindings.Add(new CommandBinding(
                ApplicationCommands.Close,
                Close_Executed,
                Close_CanExecute
            ));
        }

        private void LoadView(AppView v)
        {
            if (BodyHost.Content is ISyncToWeek syncPrev)
                syncPrev.SyncIntoWeek();
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
                        TopBarControl.Title = "Informe semanal CMC";
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

        // ====== Guardar (TopBar - ApplicationCommands.Save) ======

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Habilita si existe un proyecto cargado (y opcionalmente si hay cambios)
            e.CanExecute = ProjectStorageService.CurrentData != null;
            // e.CanExecute = ProjectStorageService.CurrentData != null && ProjectStorageService.IsDirty;
        }

        private async void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // 1) UI -> WeekData (si la vista lo implementa)
            if (BodyHost.Content is ISyncToWeek sync)
                sync.SyncIntoWeek();

            // 2) Reflejar esos cambios en el AppData actual
            MirrorWeekIntoCurrentRoot();

            // 3) Persistir
            try
            {
                var ok = await ProjectStorageService.SaveOrPromptAsync(this);
                if (!ok) return; // usuario canceló o no se guardó

                // 4) ✅ Mensaje de éxito SOLO si se guardó correctamente
                MessageBox.Show(
                    this,
                    "Guardado correctamente.",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (OperationCanceledException)
            {
                // cancelado: no mostramos nada
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"No se pudo guardar.\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // ====== Cerrar (botón Salir del TopBar) ======

        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Siempre dejamos cerrar; la confirmación se hace dentro
            e.CanExecute = true;
        }

        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close(); // 👉 esto dispara el evento Closing y ahí se muestra el MessageBox
        }

        // ====== Lógica compartida para confirmar guardado al cerrar ======

        private async Task HandleClosingConfirmAsync(CancelEventArgs e)
        {
            if (!ProjectStorageService.IsDirty)
                return; // nada que preguntar

            var r = MessageBox.Show(this,
                "Hay cambios sin guardar.\n\n¿Deseas guardarlos antes de salir?",
                "Cambios sin guardar",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (r == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (r == MessageBoxResult.Yes)
            {
                try
                {
                    // Volcar posibles cambios pendientes de la vista actual
                    if (BodyHost.Content is ISyncToWeek sync)
                        sync.SyncIntoWeek();

                    // Reflejar WeekData -> AppData root
                    MirrorWeekIntoCurrentRoot();

                    var ok = await ProjectStorageService.SaveOrPromptAsync(this);
                    if (!ok) e.Cancel = true; // canceló el diálogo de guardar
                }
                catch (OperationCanceledException)
                {
                    e.Cancel = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"No se pudo guardar.\n{ex.Message}",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                }
            }
            // Si r == No → no guardamos y dejamos cerrar
        }

        // Llamado automáticamente al cerrar por la X de la ventana
        private async void OnShellClosing_ConfirmSave(object? sender, CancelEventArgs e)
        {
            await HandleClosingConfirmAsync(e);
        }

        // Reflejar WeekData en el AppData actual
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

            // 🔹 Lo que ya tenías
            if (_week.PersonalVigente != null) wk.PersonalVigente = _week.PersonalVigente;
            if (_week.PrecursorSif != null) wk.PrecursorSif = _week.PrecursorSif;
            if (_week.Incidentes != null) wk.Incidentes = _week.Incidentes;

            // 🔹 NUEVO: Informe Semanal + Pirámide
            if (_week.InformeSemanalCma != null) wk.InformeSemanalCma = _week.InformeSemanalCma;
            if (_week.PiramideSeguridad != null) wk.PiramideSeguridad = _week.PiramideSeguridad;
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
