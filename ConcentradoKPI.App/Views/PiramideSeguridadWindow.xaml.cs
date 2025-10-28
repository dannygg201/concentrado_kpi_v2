// Views/PiramideSeguridadWindow.xaml.cs
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.ViewModels;
using System.Windows.Input;
using ConcentradoKPI.App.Services;

namespace ConcentradoKPI.App.Views
{
    public partial class PiramideSeguridadWindow : Window
    {
        private readonly Company? _company;
        private readonly Project? _project;
        private readonly WeekData? _week;
        private ShellViewModel? _shell;

        // ===== Constructor SIN parámetros (diseñador / pruebas) =====
        public PiramideSeguridadWindow()
        {
            InitializeComponent();

            // En diseñador: pon un VM “dummy” para poder ver la UI.
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new PiramideSeguridadViewModel(
                    c: new Company { Name = "Demo Co." },
                    p: new Project { Name = "Proyecto Demo" },
                    w: new WeekData()
                );
                return;
            }

            // En runtime, si alguien abre esta ventana sin pasar c/p/w:
            if (DataContext is not PiramideSeguridadViewModel)
            {
                DataContext = new PiramideSeguridadViewModel(
                    c: new Company { Name = "N/A" },
                    p: new Project { Name = "N/A" },
                    w: new WeekData()
                );
            }

            // Intenta configurar el Shell si existe como recurso
            _shell = Resources["Shell"] as ShellViewModel;
            if (_shell != null)
            {
                _shell.CurrentView = AppView.PiramideSeguridad;
                _shell.NavigateRequested += OnNavigateRequested;
            }
        }

        // ===== Constructor CON parámetros =====
        public PiramideSeguridadWindow(Company c, Project p, WeekData w)
        {
            InitializeComponent();
            _company = c;
            _project = p;
            _week = w;

            // Fija el ViewModel si no viene ya desde XAML
            if (DataContext is not PiramideSeguridadViewModel)
            {
                DataContext = new PiramideSeguridadViewModel(c, p, w);
            }

            _shell = Resources["Shell"] as ShellViewModel;
            if (_shell != null)
            {
                _shell.CurrentView = AppView.PiramideSeguridad;
                _shell.NavigateRequested += OnNavigateRequested;
            }
            // Si la semana ya trae PiramideSeguridad guardada, precárgala a la UI
            if (_week?.PiramideSeguridad is PiramideSeguridadDocument docExistente)
            {
                ApplyValuesToVm(FromDocument(docExistente));
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_shell != null)
                _shell.NavigateRequested -= OnNavigateRequested;

            base.OnClosed(e);
        }

        // ===== Navegación superior =====
        private void OnNavigateRequested(AppView target)
        {
            if (_company is null || _project is null || _week is null) return;
            
            SyncWeekFromVm();

            Window next = target switch
            {
                AppView.PersonalVigente => new PersonalVigenteWindow(_company, _project, _week),
                AppView.InformeSemanalCma => new InformeSemanalCmaWindow(_company, _project, _week),
                AppView.PrecursorSif => new PrecursorSifWindow(_company, _project, _week),
                AppView.Incidentes => new IncidentesWindow(_company, _project, _week),
                _ => null!
            };
            if (next == null) return;

            // ❌ NADA de Owner aquí (no: next.Owner = this;)

            next.Show();

            // Si esta ventana es la MainWindow y tu ShutdownMode es OnMainWindowClose,
            // pásale el rol a la nueva ANTES de cerrar para que no se termine la app.
            if (ReferenceEquals(Application.Current.MainWindow, this))
                Application.Current.MainWindow = next;

            Close();
        }

        // ===== Botón overlay: ✏️ Editar datos =====
        private void EditarDatos_Click(object sender, RoutedEventArgs e)
        {
            var current = GetValuesFromVm();
            var dlg = new PiramideEditDialog(current)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (dlg.ShowDialog() == true)
                ApplyValuesToVm(dlg.Result);
        }


        // =================== LECTURA VM → DTO ===================
        private PiramideValues GetValuesFromVm()
        {
            var vm = DataContext!;
            return new PiramideValues
            {
                // laterales / generales
                Companias = GetIntProp(vm, "Companias"),
                Colaboradores = GetIntProp(vm, "Colaboradores"),
                TecnicosSeguridad = GetIntProp(vm, "TecnicosSeguridad"),
                HorasTrabajadas = GetIntProp(vm, "HorasTrabajadas"),
                WithoutLTIs = GetIntProp(vm, "WithoutLTIs"),
                LastRecord = GetStringProp(vm, "LastRecord"),

                // base
                Seguros = GetIntProp(vm, "Seguros"),
                Inseguros = GetIntProp(vm, "Inseguros"),
                Detectadas = GetIntProp(vm, "Detectadas"),
                Corregidas = GetIntProp(vm, "Corregidas"),
                Avance = GetIntProp(vm, "Avance"),
                AvanceProgramaPct = GetIntProp(vm, "AvanceProgramaPct"),
                Efectividad = GetIntProp(vm, "Efectividad"),
                TerritoriosRojo = GetIntProp(vm, "TerritoriosRojo"),
                TerritoriosVerde = GetIntProp(vm, "TerritoriosVerde"),

                // centro
                Potenciales = GetIntProp(vm, "Potenciales"),
                Precursores1 = GetIntProp(vm, "Precursores1"),
                Precursores2 = GetIntProp(vm, "Precursores2"),
                Precursores3 = GetIntProp(vm, "Precursores3"),

                // incidentes sin lesión
                IncidentesSinLesion1 = GetIntProp(vm, "IncidentesSinLesion1"),
                IncidentesSinLesion2 = GetIntProp(vm, "IncidentesSinLesion2"),

                // niveles
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

        // =================== APLICAR DTO → VM ===================
        private void ApplyValuesToVm(PiramideValues v)
        {
            var vm = DataContext!;

            // laterales / generales
            SetIntProp(vm, "Companias", v.Companias);
            SetIntProp(vm, "Colaboradores", v.Colaboradores);
            SetIntProp(vm, "TecnicosSeguridad", v.TecnicosSeguridad);
            SetIntProp(vm, "HorasTrabajadas", v.HorasTrabajadas);
            SetIntProp(vm, "WithoutLTIs", v.WithoutLTIs);
            SetStringProp(vm, "LastRecord", v.LastRecord ?? "");

            // base
            SetIntProp(vm, "Seguros", v.Seguros);
            SetIntProp(vm, "Inseguros", v.Inseguros);
            SetIntProp(vm, "Detectadas", v.Detectadas);
            SetIntProp(vm, "Corregidas", v.Corregidas);
            SetIntProp(vm, "Avance", v.Avance);
            SetIntProp(vm, "AvanceProgramaPct", v.AvanceProgramaPct);
            SetIntProp(vm, "Efectividad", v.Efectividad);
            SetIntProp(vm, "TerritoriosRojo", v.TerritoriosRojo);
            SetIntProp(vm, "TerritoriosVerde", v.TerritoriosVerde);

            // centro
            SetIntProp(vm, "Potenciales", v.Potenciales);
            SetIntProp(vm, "Precursores1", v.Precursores1);
            SetIntProp(vm, "Precursores2", v.Precursores2);
            SetIntProp(vm, "Precursores3", v.Precursores3);

            // incidentes sin lesión
            SetIntProp(vm, "IncidentesSinLesion1", v.IncidentesSinLesion1);
            SetIntProp(vm, "IncidentesSinLesion2", v.IncidentesSinLesion2);

            // niveles
            SetIntProp(vm, "FAI1", v.FAI1); SetIntProp(vm, "FAI2", v.FAI2); SetIntProp(vm, "FAI3", v.FAI3);
            SetIntProp(vm, "MTI1", v.MTI1); SetIntProp(vm, "MTI2", v.MTI2); SetIntProp(vm, "MTI3", v.MTI3);
            SetIntProp(vm, "MDI1", v.MDI1); SetIntProp(vm, "MDI2", v.MDI2); SetIntProp(vm, "MDI3", v.MDI3);
            SetIntProp(vm, "LTI1", v.LTI1); SetIntProp(vm, "LTI2", v.LTI2); SetIntProp(vm, "LTI3", v.LTI3);
        }

        // === Habilitación del comando Guardar ===
        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Aquí podrías validar algo; por ahora, siempre habilitado
            e.CanExecute = true;
        }

        // === Guardar al archivo del proyecto/semana ===
        private async void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_company is null || _project is null || _week is null)
            {
                MessageBox.Show("No hay contexto de semana/proyecto para guardar.", "Guardar",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 1) VM -> WeekData (memoria)
                SyncWeekFromVm();

                // 2) Persistir al archivo (elige ubicación si es nuevo)
                var path = await ProjectStorageService.SaveOrPromptAsync(this);

                MessageBox.Show($"Guardado correctamente.\n\nArchivo:\n{path}",
                                "Guardar", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (OperationCanceledException) { /* cancelado por el usuario */ }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo guardar.\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // === Llevar los valores actuales de la UI a WeekData.PiramideSeguridad ===
        private void SyncWeekFromVm()
        {
            if (_company is null || _project is null || _week is null) return;

            // lee lo que está en pantalla
            var values = GetValuesFromVm();

            // opcional: normaliza rangos
            if (values.AvanceProgramaPct < 0) values.AvanceProgramaPct = 0;
            if (values.AvanceProgramaPct > 100) values.AvanceProgramaPct = 100;

            // conviértelo al documento serializable y súbelo a la semana
            _week.PiramideSeguridad = ToDocument(values, _company.Name, _project.Name, _week.WeekNumber);
        }


        // ====================== Helpers de conversión ======================
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
            try { return Convert.ToInt32(val, CultureInfo.InvariantCulture); }
            catch { return 0; }
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
                if (t == typeof(string))
                    p.SetValue(target, value.ToString(CultureInfo.InvariantCulture));
                else if (t == typeof(int))
                    p.SetValue(target, value);
                else if (t == typeof(long))
                    p.SetValue(target, (long)value);
                else if (t == typeof(double))
                    p.SetValue(target, (double)value);
                else if (t == typeof(decimal))
                    p.SetValue(target, (decimal)value);
                else
                    p.SetValue(target, Convert.ChangeType(value, t, CultureInfo.InvariantCulture));
            }
            catch { /* ignora si no se puede convertir */ }
        }

        private static void SetStringProp(object target, string propName, string value)
        {
            var p = target.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p == null || !p.CanWrite) return;

            var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
            try
            {
                if (t == typeof(string))
                    p.SetValue(target, value);
                else if (t == typeof(DateTime))
                {
                    if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                        p.SetValue(target, dt);
                }
                else
                {
                    p.SetValue(target, Convert.ChangeType(value, t, CultureInfo.InvariantCulture));
                }
            }
            catch { /* ignora si no se puede convertir */ }
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
    }
}
