using System;
using System.ComponentModel;
using System.Windows;
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.ViewModels;

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
                // Dummies mínimos si alguien abre sin c/p/w en runtime
                var c = new Company { Name = "N/A" };
                var p = new Project { Name = "N/A" };
                var w = new WeekData { WeekNumber = 1 };
                DataContext = new InformeSemanalCMAViewModel(c, p, w, nombreInicial: c.Name);
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

            // ✅ ahora el TopBar ve Company/Project/Week
            DataContext = new InformeSemanalCMAViewModel(c, p, w, nombreInicial: c?.Name ?? "", especialidadInicial: "");

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
    }
}
