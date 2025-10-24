using System.Windows;
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.ViewModels;

namespace ConcentradoKPI.App.Views
{
    public partial class IncidentesWindow : Window
    {
        private readonly Company _company;
        private readonly Project _project;
        private readonly WeekData _week;

        public IncidentesWindow(Company c, Project p, WeekData w)
        {
            InitializeComponent();
            _company = c; _project = p; _week = w;

            // VM
            DataContext = new IncidentesViewModel(
                semana: w?.ToString() ?? "Semana",
                proyecto: p?.Name ?? "Proyecto"
            );

            // Mantener NavBar funcionando como en las otras vistas
            var shell = (ShellViewModel)Resources["Shell"];
            shell.CurrentView = AppView.Incidentes;
            shell.NavigateRequested += target =>
            {
                switch (target)
                {
                    case AppView.PersonalVigente: new PersonalVigenteWindow(_company, _project, _week).Show(); Close(); break;
                    case AppView.PiramideSeguridad: new PiramideSeguridadWindow(_company, _project, _week).Show(); Close(); break;
                    case AppView.InformeSemanalCma: new InformeSemanalCmaWindow(_company, _project, _week).Show(); Close(); break;
                    case AppView.PrecursorSif: new PrecursorSifWindow(_company, _project, _week).Show(); Close(); break;
                }
            };
        }
    }
}
