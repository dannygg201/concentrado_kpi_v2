using System;
using System.ComponentModel;
using System.Windows;
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.ViewModels;
using ConcentradoKPI.App.Services;

namespace ConcentradoKPI.App.Views
{
    public partial class InformeSemanalCmaWindow : Window
    {
        private readonly Company? _company;
        private readonly Project? _project;
        private readonly WeekData? _week;
        private ShellViewModel? _shell;

        // ---- Constructor sin parámetros (diseño / prueba) ----
        public InformeSemanalCmaWindow()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new InformeSemanalCMAViewModel(
                    semana: "1",
                    proyecto: "Proyecto Demo",
                    nombreInicial: "Contratista Demo",
                    especialidadInicial: "Otros");
            }
            else
            {
                var c = new Company { Name = "N/A" };
                var p = new Project { Name = "N/A" };
                var w = new WeekData { WeekNumber = 1 };

                DataContext = new InformeSemanalCMAViewModel(c, p, w, nombreInicial: c.Name);

                // 🔔 Ping al cargar
                Loaded += (_, __) =>
                {
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeAsync(
                        () => w.Live.NotifyAll(),
                        System.Windows.Threading.DispatcherPriority.Background
                    );
                };
            }

            WireShell();
        }


        // ---- Constructor real (con c/p/w) ----
        public InformeSemanalCmaWindow(Company c, Project p, WeekData w)
        {
            InitializeComponent();
            _company = c;
            _project = p;
            _week = w;

            DataContext = new InformeSemanalCMAViewModel(c, p, w, nombreInicial: c?.Name ?? "", especialidadInicial: "");

            // 🔔 Ping al cargar
            Loaded += (_, __) =>
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeAsync(
                    () => _week!.Live.NotifyAll(),
                    System.Windows.Threading.DispatcherPriority.Background
                );
            };

            HydrateFromWeek();
            WireShell();
        }


        private void WireShell()
        {
            _shell = Resources["Shell"] as ShellViewModel;
            if (_shell == null) return;

            _shell.CurrentView = AppView.InformeSemanalCma;
            _shell.NavigateRequested -= OnNavigateRequested; // evita doble suscripción
            _shell.NavigateRequested += OnNavigateRequested;
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_shell != null)
                _shell.NavigateRequested -= OnNavigateRequested;
            base.OnClosed(e);
        }

        // Navegación del menú superior
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

            // Si esta ventana es MainWindow, pásale el rol para no cerrar la app
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

        // ===== Volcar desde el VM.Editable hacia WeekData.InformeSemanalCma =====
        private void SyncWeekFromVm()
        {
            if (DataContext is not InformeSemanalCMAViewModel vm) return;
            if (_company is null || _project is null || _week is null) return;

            var e = vm.Editable;

            _week.InformeSemanalCma = new InformeSemanalCmaDocument
            {
                Company = _company.Name,
                Project = _project.Name,
                WeekNumber = _week.WeekNumber,

                Nombre = e.Nombre ?? "",
                Especialidad = e.Especialidad ?? "",

                TecnicosSeguridad = e.TecnicosSeguridad,
                Colaboradores = e.Colaboradores,
                HorasTrabajadas = e.HorasTrabajadas,

                LTI = e.LTI,
                MDI = e.MDI,
                MTI = e.MTI,
                TRI = e.TRI,
                FAI = e.FAI,

                Incidentes = e.Incidentes,

                PrecursoresSifComportamiento = e.PrecursoresSifComportamiento,
                PrecursoresSifCondicion = e.PrecursoresSifCondicion,

                ActosSeguros = e.ActosSeguros,
                ActosInseguros = e.ActosInseguros,

                TotalSemanal = e.TotalSemanal,
                PorcentajeAvance = e.PorcentajeAvance,

                SavedUtc = DateTime.UtcNow
            };
        }
        // ===== Cargar VM desde WeekData.InformeSemanalCma (si existe) =====
        private void HydrateFromWeek()
        {
            if (_week?.InformeSemanalCma is not InformeSemanalCmaDocument d) return;
            if (DataContext is not InformeSemanalCMAViewModel vm) return;

            var e = vm.Editable;

            // Campos base
            e.Nombre = d.Nombre ?? "";
            e.Especialidad = d.Especialidad ?? "";

            e.TecnicosSeguridad = d.TecnicosSeguridad;
            e.Colaboradores = d.Colaboradores;
            e.HorasTrabajadas = d.HorasTrabajadas;

            // Indicadores
            e.LTI = d.LTI; e.MDI = d.MDI; e.MTI = d.MTI; e.TRI = d.TRI; e.FAI = d.FAI;

            // Incidentes y precursores
            e.Incidentes = d.Incidentes;
            e.PrecursoresSifComportamiento = d.PrecursoresSifComportamiento;
            e.PrecursoresSifCondicion = d.PrecursoresSifCondicion;

            // Actos
            e.ActosSeguros = d.ActosSeguros;
            e.ActosInseguros = d.ActosInseguros;

            // Forzamos recálculo de derivados en la fila de totales
            // (el VM ya recalcula al cambiar propiedades, pero por si acaso):
            vm.GetType().GetMethod("RecalcularTotales",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(vm, null);
        }

    }
}
