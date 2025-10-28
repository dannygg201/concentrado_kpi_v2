using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.ViewModels;
using ConcentradoKPI.App.Services;

namespace ConcentradoKPI.App.Views
{
    public partial class IncidentesWindow : Window
    {
        private readonly Company? _company;
        private readonly Project? _project;
        private readonly WeekData? _week;

        public IncidentesWindow()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                // VM de diseño (para que el TopBar tenga datos en diseñador)
                var c = new Company { Name = "Demo Co." };
                var p = new Project { Name = "Proyecto Demo" };
                var w = new WeekData { WeekNumber = 1 };
                DataContext = new IncidentesViewModel(c, p, w);
            }
            else
            {
                // Fallback mínimo si alguien abre sin c/p/w en runtime
                var c = new Company { Name = "N/A" };
                var p = new Project { Name = "N/A" };
                var w = new WeekData { WeekNumber = 1 };
                DataContext = new IncidentesViewModel(c, p, w);
            }

            // NavBar (si tienes Shell en Window.Resources)
            if (Resources["Shell"] is ShellViewModel shell)
            {
                shell.CurrentView = AppView.Incidentes;
                shell.NavigateRequested += OnNavigateRequested;
            }
        }

        public IncidentesWindow(Company c, Project p, WeekData w)
        {
            InitializeComponent();
            _company = c; _project = p; _week = w;

            // ✅ VM real con c/p/w (TopBar lee Company/Project/Week)
            DataContext = new IncidentesViewModel(c, p, w);
            HydrateFromWeek();

            if (Resources["Shell"] is ShellViewModel shell)
            {
                shell.CurrentView = AppView.Incidentes;
                shell.NavigateRequested += OnNavigateRequested;
            }
        }

        private void OnNavigateRequested(AppView target)
        {
            if (_company is null || _project is null || _week is null) return;

            Window next = target switch
            {
                AppView.PersonalVigente => new PersonalVigenteWindow(_company, _project, _week),
                AppView.PiramideSeguridad => new PiramideSeguridadWindow(_company, _project, _week),
                AppView.InformeSemanalCma => new InformeSemanalCmaWindow(_company, _project, _week),
                AppView.PrecursorSif => new PrecursorSifWindow(_company, _project, _week),
                AppView.Incidentes => new IncidentesWindow(_company, _project, _week),
                _ => null!
            };
            if (next == null) return;

            next.Show();

            // Si esta ventana es MainWindow, pásale el rol antes de cerrar para no terminar la app
            if (ReferenceEquals(Application.Current.MainWindow, this))
                Application.Current.MainWindow = next;

            Close();
        }
        // ===== Guardar (habilitado) =====
        private void SaveCommand_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _company != null && _project != null && _week != null;
        }

        // ===== Guardar (ejecución) =====
        private async void SaveCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (_company is null || _project is null || _week is null)
            {
                MessageBox.Show("No hay contexto de semana/proyecto para guardar.", "Guardar",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 1) VM -> WeekData (memoria)
                SyncWeekFromVm();

                // 2) Persistir al archivo (elige ubicación si es nuevo)
                var path = await ProjectStorageService.SaveOrPromptAsync(this);

                MessageBox.Show($"Guardado correctamente.\n\nArchivo:\n{path}",
                                "Guardar", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (OperationCanceledException) { /* cancelado por el usuario */ }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo guardar.\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // ===== Volcar VM.Registros → WeekData.Incidentes =====
        private void SyncWeekFromVm()
        {
            if (DataContext is not IncidentesViewModel vm) return;
            if (_company is null || _project is null || _week is null) return;

            var snapshot = vm.Registros
                             .Select(r => r.Clone())
                             .ToList();

            _week.Incidentes = new IncidentesDocument
            {
                Company = _company.Name,
                Project = _project.Name,
                WeekNumber = _week.WeekNumber,
                Items = snapshot,
                SavedUtc = DateTime.UtcNow
            };
        }
        // ===== Cargar WeekData.Incidentes → VM.Registros al abrir =====
        private void HydrateFromWeek()
        {
            if (_week?.Incidentes is not IncidentesDocument d) return;
            if (DataContext is not IncidentesViewModel vm) return;

            vm.Registros.Clear();
            foreach (var it in d.Items)
                vm.Registros.Add(it.Clone());

            // Renumerar consecutivo 1..N por si acaso
            for (int i = 0; i < vm.Registros.Count; i++)
                vm.Registros[i].No = i + 1;

            // Limpia selección y formulario
            vm.Seleccionado = null;
            vm.Form = new IncidentRecord();
        }

    }
}
