using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.ViewModels;
using ConcentradoKPI.App.Services;
using ConcentradoKPI.App.Views.Abstractions; // ⬅️ ISyncToWeek

namespace ConcentradoKPI.App.Views.Pages
{
    public partial class PrecursorSifView : UserControl, ISyncToWeek
    {
        private Company? _company;
        private Project? _project;
        private WeekData? _week;

        public PrecursorSifView()
        {
            InitializeComponent();

            // ⚠️ Solo en diseñador cargamos datos dummy.
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                var c = new Company { Name = "Demo Co." };
                var p = new Project { Name = "Proyecto Demo" };
                var w = new WeekData { WeekNumber = 1 };
                DataContext = new PrecursorSifViewModel(c, p, w);
            }

            Loaded += OnLoaded;
        }

        public PrecursorSifView(Company c, Project p, WeekData w) : this()
        {
            _company = c;
            _project = p;
            _week = w;
            DataContext = new PrecursorSifViewModel(c, p, w);
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            // Si el Shell asignó el DataContext, resolvemos c/p/w desde el VM
            if ((_company, _project, _week) == (null, null, null) &&
                DataContext is PrecursorSifViewModel vm)
            {
                _company = vm.Company;
                _project = vm.Project;
                _week = vm.Week;
            }

            // Hidratar desde WeekData al abrir (si hay persistencia)
            HydrateFromWeek();

            // Opcional: "ping" de Live si tu VM escucha métricas en tiempo real
            _week?.Live?.NotifyAll();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Puedes dejarlo vacío o agregar lógica si lo usas.
        }

        // ======= ISyncToWeek: llamado por ShellWindow antes de guardar =======
        public void SyncIntoWeek()
        {
            if (DataContext is not PrecursorSifViewModel vm) return;
            if (_company is null || _project is null || _week is null) return;

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

            for (int i = 0; i < vm.Registros.Count; i++)
                vm.Registros[i].No = i + 1;

            vm.Seleccionado = null;
            if (vm.Form is not null)
                vm.Form.CopyFrom(new PrecursorSifRecord());
        }
    }
}
