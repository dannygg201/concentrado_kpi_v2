using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.ViewModels;
using ConcentradoKPI.App.Services;
using ConcentradoKPI.App.Views.Abstractions; // ISyncToWeek

namespace ConcentradoKPI.App.Views.Pages
{
    public partial class IncidentesView : UserControl, ISyncToWeek
    {
        private Company? _company;
        private Project? _project;
        private WeekData? _week;

        public IncidentesView()
        {
            InitializeComponent();

            // Solo en diseñador cargamos dummy VM
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                var c = new Company { Name = "Demo Co." };
                var p = new Project { Name = "Proyecto Demo" };
                var w = new WeekData { WeekNumber = 1 };
                DataContext = new IncidentesViewModel(c, p, w);
            }

            Loaded += OnLoaded;
        }

        public IncidentesView(Company c, Project p, WeekData w) : this()
        {
            _company = c;
            _project = p;
            _week = w;

            DataContext = new IncidentesViewModel(c, p, w);
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            // Resolver c/p/w desde el VM si el Shell ya puso DataContext
            if ((_company, _project, _week) == (null, null, null) &&
                DataContext is IncidentesViewModel vm)
            {
                _company = vm.Company;
                _project = vm.Project;
                _week = vm.Week;
            }

            HydrateFromWeek();
            _week?.Live?.NotifyAll();
        }

        // ======= Shell llamará esto antes de persistir =======
        public void SyncIntoWeek()
        {
            if (DataContext is not IncidentesViewModel vm) return;
            if (_company is null || _project is null || _week is null) return;

            var snapshot = vm.Registros?.Select(r => r.Clone()).ToList() ?? new();

            _week.Incidentes = new IncidentesDocument
            {
                Company = _company.Name,
                Project = _project.Name,
                WeekNumber = _week.WeekNumber,
                Items = snapshot,
                SavedUtc = DateTime.UtcNow
            };
        }

        // ===== Persistido -> VM =====
        private void HydrateFromWeek()
        {
            if (_week?.Incidentes is not IncidentesDocument d) return;
            if (DataContext is not IncidentesViewModel vm) return;

            vm.Registros.Clear();
            foreach (var it in d.Items)
                vm.Registros.Add(it.Clone());

            for (int i = 0; i < vm.Registros.Count; i++)
                vm.Registros[i].No = i + 1;

            vm.Seleccionado = null;
            vm.Form = new IncidentRecord();
        }
    }
}
