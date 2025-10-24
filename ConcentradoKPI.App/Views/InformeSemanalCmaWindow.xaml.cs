using System;
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

            // VM demo
            DataContext = new InformeSemanalCMAViewModel(
                semana: "Semana 1",
                proyecto: "Proyecto 1",
                nombreInicial: "Contratista Demo",
                especialidadInicial: "Otros");

            // Cablea NavBar (no habrá navegación real porque no tenemos c/p/w)
            WireShell();
        }

        // ---- Constructor real (con c/p/w) ----
        public InformeSemanalCmaWindow(Company c, Project p, WeekData w)
        {
            InitializeComponent();
            _company = c;
            _project = p;
            _week = w;

            DataContext = new InformeSemanalCMAViewModel(
                semana: w?.ToString() ?? "Semana",
                proyecto: p?.Name ?? "Proyecto",
                nombreInicial: c?.Name ?? "",
                especialidadInicial: "");

            WireShell();
        }

        private void WireShell()
        {
            // Recupera el Shell del XAML (Window.Resources)
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
            // Si abriste la ventana sin c/p/w, no navegues
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

            if (next != null)
            {
                next.Show();
                Close();
            }
        }
    }
}
