using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.ViewModels;
using ConcentradoKPI.App.Services;

namespace ConcentradoKPI.App.Views
{
    public partial class PrecursorSifWindow : Window
    {
        private readonly Company? _company;
        private readonly Project? _project;
        private readonly WeekData? _week;

        public PrecursorSifWindow()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                // VM de diseño (dummies para que el TopBar tenga datos en diseñador)
                var c = new Company { Name = "Demo Co." };
                var p = new Project { Name = "Proyecto Demo" };
                var w = new WeekData { WeekNumber = 1 };
                DataContext = new PrecursorSifViewModel(c, p, w);
            }
            else
            {
                // Runtime sin parámetros (fallback mínimo)
                var c = new Company { Name = "N/A" };
                var p = new Project { Name = "N/A" };
                var w = new WeekData { WeekNumber = 1 };
                DataContext = new PrecursorSifViewModel(c, p, w);
            }

            // Shell / NavBar (si existe en Window.Resources)
            if (Resources["Shell"] is ShellViewModel shell)
            {
                shell.CurrentView = AppView.PrecursorSif;
                shell.NavigateRequested += OnNavigateRequested;
            }
        }

        public PrecursorSifWindow(Company c, Project p, WeekData w)
        {
            InitializeComponent();
            _company = c; _project = p; _week = w;

            // ✅ VM real con c/p/w para que el TopBar lea Company/Project/Week
            DataContext = new PrecursorSifViewModel(c, p, w);
            HydrateFromWeek();

            if (Resources["Shell"] is ShellViewModel shell)
            {
                shell.CurrentView = AppView.PrecursorSif;
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

            // Si esta ventana es la MainWindow, pásale el rol antes de cerrar
            if (ReferenceEquals(Application.Current.MainWindow, this))
                Application.Current.MainWindow = next;

            Close();
        }

        private void DataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

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

        // ===== VM → WeekData.PrecursorSif =====
        private void SyncWeekFromVm()
        {
            if (DataContext is not PrecursorSifViewModel vm) return;
            if (_company is null || _project is null || _week is null) return;

            // Toma la lista actual del VM
            var items = vm.Registros?.Select(r => r.Clone()).ToList() ?? new();

            _week.PrecursorSif = new PrecursorSifDocument
            {
                Company = _company.Name,
                Project = _project.Name,
                WeekNumber = _week.WeekNumber,
                Items = items,
                SavedUtc = DateTime.UtcNow
            };
        }

        // ===== Cargar VM desde WeekData.PrecursorSif (si existe) =====
        private void HydrateFromWeek()
        {
            if (_week?.PrecursorSif is not PrecursorSifDocument d) return;
            if (DataContext is not PrecursorSifViewModel vm) return;

            vm.Registros.Clear();
            foreach (var it in d.Items)
                vm.Registros.Add(it.Clone());

            // Renumerar el consecutivo (No = 1..N)
            for (int i = 0; i < vm.Registros.Count; i++)
                vm.Registros[i].No = i + 1;

            // Limpia selección y el formulario si aplica
            vm.Seleccionado = null;
            if (vm.Form is not null)
                vm.Form.CopyFrom(new PrecursorSifRecord()); // deja el form en blanco
        }



    }
}
