using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.ViewModels;
using ConcentradoKPI.App.Views.Abstractions; // ISyncToWeek
using ConcentradoKPI.App.Services; // ⬅️ para LastEspecialidadStore

namespace ConcentradoKPI.App.Views
{
    public partial class InformeSemanalCmaView : UserControl, ISyncToWeek
    {
        private Company? _company;
        private Project? _project;
        private WeekData? _week;

        // ✅ Ctor para diseñador / host sin DI (el Shell le pondrá DataContext real)
        public InformeSemanalCmaView()
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

            Loaded += OnLoaded;
        }

        // ✅ Ctor práctico si instancias el view manualmente
        public InformeSemanalCmaView(Company c, Project p, WeekData w) : this()
        {
            _company = c;
            _project = p;
            _week = w;

            DataContext = new InformeSemanalCMAViewModel(
                c, p, w,
                nombreInicial: c?.Name ?? string.Empty,
                especialidadInicial: string.Empty);
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            // Resolver c/p/w desde el VM si no vinieron por ctor
            if ((_company, _project, _week) == (null, null, null) &&
                DataContext is InformeSemanalCMAViewModel vm)
            {
                _company = vm.Company;
                _project = vm.Project;
                _week = vm.Week;
            }

            // 🔔 Dispara estado inicial de Live para que el VM haga pull
            _week?.Live?.NotifyAll();
        }

        // ======= ISyncToWeek: el Shell llamará esto antes de guardar =======
        public void SyncIntoWeek()
        {
            if (DataContext is not InformeSemanalCMAViewModel vm) return;
            if (_company is null || _project is null || _week is null) return;

            var e = vm.Editable;

            _week.InformeSemanalCma = new InformeSemanalCmaDocument
            {
                Company = _company.Name,
                Project = _project.Name,
                WeekNumber = _week.WeekNumber,

                Nombre = e.Nombre ?? string.Empty,
                Especialidad = e.Especialidad ?? string.Empty,

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

            // 🔸 Solo persistimos "último valor" si NO está vacío (evita borrar el previo)
            if (!string.IsNullOrWhiteSpace(e.Especialidad))
            {
                LastEspecialidadStore.Set(_company.Name, _project.Name, e.Especialidad);
            }
        }
    }
}
