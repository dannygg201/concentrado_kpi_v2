using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.ViewModels;
using ConcentradoKPI.App.Views.Abstractions;
using ConcentradoKPI.App.Services;

namespace ConcentradoKPI.App.Views.Pages
{
    public partial class PiramideSeguridadView : UserControl, ISyncToWeek
    {
        private Company? _company;
        private Project? _project;
        private WeekData? _week;
        private ShellViewModel? _shell;

        public PiramideSeguridadView()
        {
            InitializeComponent();
            DataContextChanged += PiramideSeguridadView_DataContextChanged;

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                var wLocal = new WeekData();
                DataContext = new PiramideSeguridadViewModel(
                    new Company { Name = "Demo Co." },
                    new Project { Name = "Proyecto Demo" },
                    wLocal
                );
                Loaded += (_, __) =>
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeAsync(
                        () => wLocal.Live.NotifyAll(),
                        System.Windows.Threading.DispatcherPriority.Background);
            }

            _shell = Resources["Shell"] as ShellViewModel;
            if (_shell != null)
            {
                _shell.CurrentView = AppView.PiramideSeguridad;
                _shell.NavigateRequested += OnNavigateRequested;
            }
        }

        public void InitContext(Company c, Project p, WeekData w)
        {
            _company = c;
            _project = p;
            _week = w;

            if (DataContext is not PiramideSeguridadViewModel)
                DataContext = new PiramideSeguridadViewModel(c, p, w);

            Loaded += (_, __) =>
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeAsync(
                    () => _week!.Live.NotifyAll(),
                    System.Windows.Threading.DispatcherPriority.Background);

            if (_week?.PiramideSeguridad is PiramideSeguridadDocument doc)
                ApplyValuesToVm(FromDocument(doc));
        }

        private void OnNavigateRequested(AppView target) => Navigate?.Invoke(this, target);
        public event EventHandler<AppView>? Navigate;

        private void EditarDatos_Click(object sender, RoutedEventArgs e)
        {
            var owner = Window.GetWindow(this);
            var current = GetValuesFromVm();
            var dlg = new PiramideEditDialog(current)
            {
                Owner = owner,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            if (dlg.ShowDialog() == true)
                ApplyValuesToVm(dlg.Result);

            ProjectStorageService.MarkDirty();
        }

        private void PiramideSeguridadView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is PiramideSeguridadViewModel vm)
            {
                _company = vm.Company;
                _project = vm.Project;
                _week = vm.Week;

                if (_week?.PiramideSeguridad is PiramideSeguridadDocument doc)
                    ApplyValuesToVm(FromDocument(doc));
            }
        }

        // ====== ISyncToWeek ======
        public void FlushToWeek() => SyncWeekFromVm();
        public void SyncIntoWeek() => SyncWeekFromVm();

        private void SyncWeekFromVm()
        {
            if (_company is null || _project is null || _week is null) return;

            // 1) Leer lo que el usuario tiene en la vista (base actual)
            var values = GetValuesFromVm();

            // 2) Reforzar los 3 de Live
            values.Colaboradores = _week.Live.ColaboradoresTotal;
            values.TecnicosSeguridad = _week.Live.TecnicosSeguridadTotal;
            values.HorasTrabajadas = _week.Live.HorasTrabajadasTotal;

            values.AvanceProgramaPct = Math.Clamp(values.AvanceProgramaPct, 0, 100);

            // 3) Si hay InformeSemanal de esta semana, SUMARLO a la pirámide
            var weekly = _week.InformeSemanalCma;
            if (weekly != null)
            {
                // Actos
                values.Seguros += weekly.ActosSeguros;
                values.Inseguros += weekly.ActosInseguros;

                // Precursores (comportamiento) -> Precursores1
                values.Precursores1 += weekly.PrecursoresSifComportamiento;

                // Condiciones detectadas (desde “condición”)
                values.Detectadas += weekly.PrecursoresSifCondicion;
                // Corregidas no viene en el informe semanal (no se modifica aquí)

                // Incidentes sin lesión -> usa el 1er cuadro
                values.IncidentesSinLesion1 += weekly.Incidentes;

                // Lesiones/atenciones: el informe no trae desglose 1/2/3
                // Convención: sumamos todo en el nivel 1 de cada categoría
                values.FAI1 += weekly.FAI;
                values.MTI1 += weekly.MTI;
                values.MDI1 += weekly.MDI;
                values.LTI1 += weekly.LTI;

                // TRI solo es derivado (no se guarda en pirámide), así que lo ignoramos aquí
            }

            // 4) Persistir documento
            _week.PiramideSeguridad = ToDocument(values, _company.Name, _project.Name, _week.WeekNumber);
        }

        // ====== VM <-> DTO ======
        private PiramideValues GetValuesFromVm()
        {
            var vm = DataContext!;
            return new PiramideValues
            {
                Companias = GetIntProp(vm, "Companias"),
                Colaboradores = GetIntProp(vm, "Colaboradores"),
                TecnicosSeguridad = GetIntProp(vm, "TecnicosSeguridad"),
                HorasTrabajadas = GetIntProp(vm, "HorasTrabajadas"),
                WithoutLTIs = GetIntProp(vm, "WithoutLTIs"),
                LastRecord = GetStringProp(vm, "LastRecord"),
                Seguros = GetIntProp(vm, "Seguros"),
                Inseguros = GetIntProp(vm, "Inseguros"),
                Detectadas = GetIntProp(vm, "Detectadas"),
                Corregidas = GetIntProp(vm, "Corregidas"),
                Avance = GetIntProp(vm, "Avance"),
                AvanceProgramaPct = GetIntProp(vm, "AvanceProgramaPct"),
                Efectividad = GetIntProp(vm, "Efectividad"),
                TerritoriosRojo = GetIntProp(vm, "TerritoriosRojo"),
                TerritoriosVerde = GetIntProp(vm, "TerritoriosVerde"),
                Potenciales = GetIntProp(vm, "Potenciales"),
                Precursores1 = GetIntProp(vm, "Precursores1"),
                Precursores2 = GetIntProp(vm, "Precursores2"),
                Precursores3 = GetIntProp(vm, "Precursores3"),
                IncidentesSinLesion1 = GetIntProp(vm, "IncidentesSinLesion1"),
                IncidentesSinLesion2 = GetIntProp(vm, "IncidentesSinLesion2"),
                FAI1 = GetIntProp(vm, "FAI1"),
                FAI2 = GetIntProp(vm, "FAI2"),
                FAI3 = GetIntProp(vm, "FAI3"),
                MTI1 = GetIntProp(vm, "MTI1"),
                MTI2 = GetIntProp(vm, "MTI2"),
                MTI3 = GetIntProp(vm, "MTI3"),
                MDI1 = GetIntProp(vm, "MDI1"),
                MDI2 = GetIntProp(vm, "MDI2"),
                MDI3 = GetIntProp(vm, "MDI3"),
                LTI1 = GetIntProp(vm, "LTI1"),
                LTI2 = GetIntProp(vm, "LTI2"),
                LTI3 = GetIntProp(vm, "LTI3")
            };
        }

        private void ApplyValuesToVm(PiramideValues v)
        {
            var vm = DataContext!;

            // No tocar Live (se actualiza por suscripción)
            SetIntProp(vm, "Companias", v.Companias);
            SetIntProp(vm, "WithoutLTIs", v.WithoutLTIs);
            SetStringProp(vm, "LastRecord", v.LastRecord ?? "");

            SetIntProp(vm, "Seguros", v.Seguros);
            SetIntProp(vm, "Inseguros", v.Inseguros);
            SetIntProp(vm, "Detectadas", v.Detectadas);
            SetIntProp(vm, "Corregidas", v.Corregidas);
            SetIntProp(vm, "Avance", v.Avance);
            SetIntProp(vm, "AvanceProgramaPct", v.AvanceProgramaPct);
            SetIntProp(vm, "Efectividad", v.Efectividad);
            SetIntProp(vm, "TerritoriosRojo", v.TerritoriosRojo);
            SetIntProp(vm, "TerritoriosVerde", v.TerritoriosVerde);

            SetIntProp(vm, "Potenciales", v.Potenciales);
            SetIntProp(vm, "Precursores1", v.Precursores1);
            SetIntProp(vm, "Precursores2", v.Precursores2);
            SetIntProp(vm, "Precursores3", v.Precursores3);

            SetIntProp(vm, "IncidentesSinLesion1", v.IncidentesSinLesion1);
            SetIntProp(vm, "IncidentesSinLesion2", v.IncidentesSinLesion2);

            SetIntProp(vm, "FAI1", v.FAI1); SetIntProp(vm, "FAI2", v.FAI2); SetIntProp(vm, "FAI3", v.FAI3);
            SetIntProp(vm, "MTI1", v.MTI1); SetIntProp(vm, "MTI2", v.MTI2); SetIntProp(vm, "MTI3", v.MTI3);
            SetIntProp(vm, "MDI1", v.MDI1); SetIntProp(vm, "MDI2", v.MDI2); SetIntProp(vm, "MDI3", v.MDI3);
            SetIntProp(vm, "LTI1", v.LTI1); SetIntProp(vm, "LTI2", v.LTI2); SetIntProp(vm, "LTI3", v.LTI3);
        }

        public static PiramideSeguridadDocument ToDocument(PiramideValues v, string company, string project, int weekNumber)
        {
            return new PiramideSeguridadDocument
            {
                Company = company,
                Project = project,
                WeekNumber = weekNumber,
                Companias = v.Companias,
                Colaboradores = v.Colaboradores,
                TecnicosSeguridad = v.TecnicosSeguridad,
                HorasTrabajadas = v.HorasTrabajadas,
                WithoutLTIs = v.WithoutLTIs,
                LastRecord = string.IsNullOrWhiteSpace(v.LastRecord) ? null : v.LastRecord,
                Seguros = v.Seguros,
                Inseguros = v.Inseguros,
                Detectadas = v.Detectadas,
                Corregidas = v.Corregidas,
                Avance = v.Avance,
                AvanceProgramaPct = v.AvanceProgramaPct,
                Efectividad = v.Efectividad,
                TerritoriosRojo = v.TerritoriosRojo,
                TerritoriosVerde = v.TerritoriosVerde,
                Potenciales = v.Potenciales,
                Precursores1 = v.Precursores1,
                Precursores2 = v.Precursores2,
                Precursores3 = v.Precursores3,
                IncidentesSinLesion1 = v.IncidentesSinLesion1,
                IncidentesSinLesion2 = v.IncidentesSinLesion2,
                FAI1 = v.FAI1,
                FAI2 = v.FAI2,
                FAI3 = v.FAI3,
                MTI1 = v.MTI1,
                MTI2 = v.MTI2,
                MTI3 = v.MTI3,
                MDI1 = v.MDI1,
                MDI2 = v.MDI2,
                MDI3 = v.MDI3,
                LTI1 = v.LTI1,
                LTI2 = v.LTI2,
                LTI3 = v.LTI3,
                SavedUtc = DateTime.UtcNow
            };
        }

        public static PiramideValues FromDocument(PiramideSeguridadDocument d)
        {
            return new PiramideValues
            {
                Companias = d.Companias,
                Colaboradores = d.Colaboradores,
                TecnicosSeguridad = d.TecnicosSeguridad,
                HorasTrabajadas = d.HorasTrabajadas,
                WithoutLTIs = d.WithoutLTIs,
                LastRecord = d.LastRecord ?? "",
                Seguros = d.Seguros,
                Inseguros = d.Inseguros,
                Detectadas = d.Detectadas,
                Corregidas = d.Corregidas,
                Avance = d.Avance,
                AvanceProgramaPct = d.AvanceProgramaPct,
                Efectividad = d.Efectividad,
                TerritoriosRojo = d.TerritoriosRojo,
                TerritoriosVerde = d.TerritoriosVerde,
                Potenciales = d.Potenciales,
                Precursores1 = d.Precursores1,
                Precursores2 = d.Precursores2,
                Precursores3 = d.Precursores3,
                IncidentesSinLesion1 = d.IncidentesSinLesion1,
                IncidentesSinLesion2 = d.IncidentesSinLesion2,
                FAI1 = d.FAI1,
                FAI2 = d.FAI2,
                FAI3 = d.FAI3,
                MTI1 = d.MTI1,
                MTI2 = d.MTI2,
                MTI3 = d.MTI3,
                MDI1 = d.MDI1,
                MDI2 = d.MDI2,
                MDI3 = d.MDI3,
                LTI1 = d.LTI1,
                LTI2 = d.LTI2,
                LTI3 = d.LTI3
            };
        }

        // === Reflection helpers (igual que tenías) ===
        private static int CoerceInt(object? val)
        {
            if (val == null) return 0;
            var t = val.GetType();
            if (t == typeof(int)) return (int)val;
            if (t == typeof(int?)) return ((int?)val) ?? 0;
            if (t == typeof(long)) return (int)(long)val;
            if (t == typeof(long?)) return (int)(((long?)val) ?? 0L);
            if (t == typeof(double)) return (int)Math.Round((double)val);
            if (t == typeof(double?)) return (int)Math.Round(((double?)val) ?? 0d);
            if (t == typeof(decimal)) return (int)Math.Round((decimal)val);
            if (t == typeof(decimal?)) return (int)Math.Round(((decimal?)val) ?? 0m);
            if (t == typeof(string))
            {
                var s = (string)val;
                if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var i)) return i;
                if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return (int)Math.Round(d);
            }
            try { return Convert.ToInt32(val, CultureInfo.InvariantCulture); } catch { return 0; }
        }
        private static int GetIntProp(object src, string propName)
        {
            var p = src.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p == null) return 0;
            var val = p.GetValue(src);
            return CoerceInt(val);
        }
        private static string GetStringProp(object src, string propName)
        {
            var p = src.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p == null) return "";
            var v = p.GetValue(src);
            return v?.ToString() ?? "";
        }
        private static void SetIntProp(object target, string propName, int value)
        {
            var p = target.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p == null || !p.CanWrite) return;
            var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
            try
            {
                if (t == typeof(string)) p.SetValue(target, value.ToString(CultureInfo.InvariantCulture));
                else if (t == typeof(int)) p.SetValue(target, value);
                else if (t == typeof(long)) p.SetValue(target, (long)value);
                else if (t == typeof(double)) p.SetValue(target, (double)value);
                else if (t == typeof(decimal)) p.SetValue(target, (decimal)value);
                else p.SetValue(target, Convert.ChangeType(value, t, CultureInfo.InvariantCulture));
            }
            catch { }
        }
        private static void SetStringProp(object target, string propName, string value)
        {
            var p = target.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p == null || !p.CanWrite) return;
            var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
            try
            {
                if (t == typeof(string)) p.SetValue(target, value);
                else if (t == typeof(DateTime))
                {
                    if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                        p.SetValue(target, dt);
                }
                else p.SetValue(target, Convert.ChangeType(value, t, CultureInfo.InvariantCulture));
            }
            catch { }
        }
    }
}
