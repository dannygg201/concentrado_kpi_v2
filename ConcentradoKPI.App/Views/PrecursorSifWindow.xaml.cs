using System.ComponentModel;
using System.Windows;
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.ViewModels;

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
    }
}
