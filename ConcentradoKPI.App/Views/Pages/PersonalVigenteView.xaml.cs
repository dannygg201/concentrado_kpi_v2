using System;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.Services;
using ConcentradoKPI.App.ViewModels;
using ConcentradoKPI.App.Views.Abstractions;

namespace ConcentradoKPI.App.Views.Pages
{
    public partial class PersonalVigenteView : UserControl, ISyncToWeek
    {
        // ====== ISyncToWeek ======
        public void SyncIntoWeek()
        {
            if (DataContext is not PersonalVigenteViewModel vm) return;

            vm.Week.PersonalVigenteDocument = new PersonalVigenteDocument
            {
                Company = vm.Company.Name,
                Project = vm.Project.Name,
                WeekNumber = vm.Week.WeekNumber,
                RazonSocial = vm.RazonSocial,
                ResponsableObra = vm.ResponsableObra,
                RegistroIMSS = vm.RegistroIMSS,
                RFCCompania = vm.RFCCompania,
                DireccionLegal = vm.DireccionLegal,
                NumeroProveedor = vm.NumeroProveedor,
                OrdenCompra = vm.OrdenCompra,
                Fecha = vm.Fecha,
                Observaciones = vm.Observaciones,
                Personal = vm.Personas.ToList()
            };

            // 🔄 Recalcular Live y notificar
            LiveSyncService.Recalc(vm.Week, vm.Week.PersonalVigenteDocument);
        }

        public void FlushToWeek()
        {
            if (DataContext is not PersonalVigenteViewModel vm) return;

            vm.PushToWeekData();
            LiveSyncService.Recalc(vm.Week, vm.Week.PersonalVigenteDocument);
        }


        private readonly Company _company;
        private readonly Project _project;
        private readonly WeekData _week;
        private readonly PersonalVigenteViewModel _viewModel;

        // 1) ctor SIN parámetros → diseñador / fallback
        public PersonalVigenteView()
        {
            InitializeComponent();

            // dummies para satisfacer readonly
            _company = new Company { Name = "Demo Co." };
            _project = new Project { Name = "Proyecto Demo" };
            _week = new WeekData { WeekNumber = 1 };

            _viewModel = new PersonalVigenteViewModel(_company, _project, _week);
            DataContext = _viewModel;

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            Loaded += (_, __) =>
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeAsync(
                    () => _week.Live.NotifyAll(),
                    System.Windows.Threading.DispatcherPriority.Background
                );
            };
        }

        // 2) ctor REAL → runtime con contexto
        public PersonalVigenteView(Company company, Project project, WeekData week)
        {
            InitializeComponent();

            _company = company;
            _project = project;
            _week = week;

            _viewModel = new PersonalVigenteViewModel(company, project, week);
            DataContext = _viewModel;

            Loaded += (_, __) =>
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeAsync(
                    () => _week.Live.NotifyAll(),
                    System.Windows.Threading.DispatcherPriority.Background
                );
            };

        }

        // ---------------------------------------------------------
        // Precargar desde WeekData si ya había guardado
        // ---------------------------------------------------------
        private void HydrateFromWeek()
        {
            if (_week?.PersonalVigenteDocument is not PersonalVigenteDocument d)
            {
                // No hay documento previo -> Live en cero y salir
                LiveSyncService.Recalc(_week, null);
                return;
            }

            _viewModel.RazonSocial = d.RazonSocial ?? "";
            _viewModel.ResponsableObra = d.ResponsableObra ?? "";
            _viewModel.RegistroIMSS = d.RegistroIMSS ?? "";
            _viewModel.RFCCompania = d.RFCCompania ?? "";
            _viewModel.DireccionLegal = d.DireccionLegal ?? "";
            _viewModel.NumeroProveedor = d.NumeroProveedor ?? "";
            _viewModel.OrdenCompra = d.OrdenCompra;
            _viewModel.Fecha = d.Fecha;
            _viewModel.Observaciones = d.Observaciones;

            _viewModel.Personas.Clear();
            foreach (var p in (d.Personal ?? Enumerable.Empty<PersonRow>()))
                _viewModel.Personas.Add(p);

            // 🔄 Recalcula Live con lo cargado
            LiveSyncService.Recalc(_week, d);
        }

        // ---------------------------------------------------------
        // VM -> WeekData
        // ---------------------------------------------------------
        public void SyncWeekFromVm()
        {
            _week.PersonalVigente = new PersonalVigenteDocument
            {
                Company = _company.Name,
                Project = _project.Name,
                WeekNumber = _week.WeekNumber,
                RazonSocial = _viewModel.RazonSocial ?? "",
                ResponsableObra = _viewModel.ResponsableObra ?? "",
                RegistroIMSS = _viewModel.RegistroIMSS ?? "",
                RFCCompania = _viewModel.RFCCompania ?? "",
                DireccionLegal = _viewModel.DireccionLegal ?? "",
                NumeroProveedor = _viewModel.NumeroProveedor ?? "",
                OrdenCompra = _viewModel.OrdenCompra,
                Fecha = _viewModel.Fecha,
                Observaciones = _viewModel.Observaciones,
                Personal = _viewModel.Personas.ToList(),
                SavedUtc = DateTime.UtcNow
            };

            // 🔄 asegurar Live consistente con lo recién copiado al Week
            LiveSyncService.Recalc(_week, _week.PersonalVigente);
        }

        // ---------------------------------------------------------
        // Guardar: versión con owner explícito
        // ---------------------------------------------------------
        public async Task GuardarAsync(Window owner)
        {
            try
            {
                SyncWeekFromVm();
                var path = await ProjectStorageService.SaveOrPromptAsync(owner);
                MessageBox.Show($"Guardado correctamente.\n\nArchivo:\n{path}",
                                "Guardar", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (OperationCanceledException) { /* cancelado */ }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo guardar.\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ---------------------------------------------------------
        // Guardar: overload que resuelve owner automáticamente
        // (ideal para llamarlo desde la Shell)
        // ---------------------------------------------------------
        public Task GuardarAsync()
        {
            var owner = Window.GetWindow(this) ?? Application.Current.MainWindow;
            return GuardarAsync(owner!);
        }

        private async void OnGuardarClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is PersonalVigenteViewModel vm)
                {
                    // 1) Empuja los datos al Week + recalcula Live + lanza evento
                    vm.PushToWeekData();
                }

                // 2) Guarda el archivo
                var owner = Window.GetWindow(this);
                var path = await ProjectStorageService.SaveOrPromptAsync(owner);

                MessageBox.Show($"Guardado correctamente.\n\nArchivo:\n{path}",
                    "Guardar", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (OperationCanceledException)
            {
                // usuario canceló, nada
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo guardar.\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
