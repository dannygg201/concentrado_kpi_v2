using System.Windows;
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.ViewModels;

namespace ConcentradoKPI.App.Views
{
    public partial class PrecursorSifWindow : Window
    {
        private readonly Company _company;
        private readonly Project _project;
        private readonly WeekData _week;

        public PrecursorSifWindow(Company c, Project p, WeekData w)
        {
            InitializeComponent();
            _company = c; _project = p; _week = w;

            // VM real en runtime (el diseñador lo ignora por d:DataContext)
            DataContext = new PrecursorSifViewModel(
                semana: w?.ToString() ?? "Semana",
                proyecto: p?.Name ?? "Proyecto");

            // Shell / NavBar
            var shell = (ShellViewModel)Resources["Shell"];
            shell.CurrentView = AppView.PrecursorSif;

            shell.NavigateRequested += target =>
            {
                switch (target)
                {
                    case AppView.PersonalVigente:
                        new PersonalVigenteWindow(_company, _project, _week).Show(); Close(); break;
                    case AppView.PiramideSeguridad:
                        new PiramideSeguridadWindow(_company, _project, _week).Show(); Close(); break;
                    case AppView.InformeSemanalCma:
                        new InformeSemanalCmaWindow(_company, _project, _week).Show(); Close(); break;
                    case AppView.Incidentes:
                        new IncidentesWindow(_company, _project, _week).Show(); Close(); break;
                }
            };
        }
    }
}
