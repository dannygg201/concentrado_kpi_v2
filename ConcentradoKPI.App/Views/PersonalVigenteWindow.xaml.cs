// Views/PersonalVigenteWindow.xaml.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.Services;
using ConcentradoKPI.App.ViewModels;

namespace ConcentradoKPI.App.Views
{
    public partial class PersonalVigenteWindow : Window
    {
        private readonly Company _company;
        private readonly Project _project;
        private readonly WeekData _week;

        public PersonalVigenteWindow(Company c, Project p, WeekData w)
        {
            InitializeComponent();

            _company = c;
            _project = p;
            _week = w;

            // ⬇️ MUY IMPORTANTE: DataContext con el VM que hidrata la vista
            DataContext = new PersonalVigenteViewModel(c, p, w);
            // Shell de ESTA ventana (declarado en el XAML: <vm:ShellViewModel x:Key="Shell"/>)
            if (Resources["Shell"] is ShellViewModel shell)
            {
                shell.CurrentView = AppView.PersonalVigente;
                shell.NavigateRequested += OnNavigateRequested;
            }
        }

        // ===== Navegación de la NavBar (entre ventanas de la misma semana) =====
        private void OnNavigateRequested(AppView target)
        {
            switch (target)
            {
                case AppView.PiramideSeguridad:
                    new PiramideSeguridadWindow(_company, _project, _week).Show();
                    Close();
                    break;
                case AppView.InformeSemanalCma:
                    new InformeSemanalCmaWindow(_company, _project, _week).Show();
                    Close();
                    break;
                case AppView.PrecursorSif:
                    new PrecursorSifWindow(_company, _project, _week).Show();
                    Close();
                    break;
                case AppView.Incidentes:
                    new IncidentesWindow(_company, _project, _week).Show();
                    Close();
                    break;
            }
        }

        // ===== Guardar (habilitado) =====
        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Si quieres validar campos obligatorios, hazlo aquí.
            e.CanExecute = true;
        }

        // ===== Guardar (ejecución) =====
        private async void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (DataContext is not PersonalVigenteViewModel vm)
            {
                MessageBox.Show("No hay ViewModel; no se puede guardar.", "Guardar",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1) volcar a la semana (esto es lo que se serializa)
            _week.PersonalVigente = new PersonalVigenteDocument
            {
                Company = _company.Name,
                Project = _project.Name,
                WeekNumber = _week.WeekNumber,
                RazonSocial = vm.RazonSocial ?? "",
                ResponsableObra = vm.ResponsableObra ?? "",
                RegistroIMSS = vm.RegistroIMSS ?? "",
                RFCCompania = vm.RFCCompania ?? "",
                DireccionLegal = vm.DireccionLegal ?? "",
                NumeroProveedor = vm.NumeroProveedor ?? "",
                Fecha = vm.Fecha,
                Personal = vm.Personas.ToList()
            };

            try
            {
                var path = await ProjectStorageService.SaveOrPromptAsync(this); // pide ruta si es nuevo
                MessageBox.Show($"Guardado correctamente.\n\nArchivo:\n{path}",
                                "Guardar", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo guardar.\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // ===== Salir =====
        private void Salir_Click(object sender, RoutedEventArgs e) => Close();

        // ========= Helpers por reflexión (para desacoplar del nombre exacto de tus props) =========
        private static string GetString(object source, params string[] names)
        {
            foreach (var n in names)
            {
                var p = source.GetType().GetProperty(n, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                {
                    var val = p.GetValue(source);
                    if (val != null) return val.ToString() ?? "";
                }
            }
            return "";
        }

        private static DateTime? GetNullableDate(object source, params string[] names)
        {
            foreach (var n in names)
            {
                var p = source.GetType().GetProperty(n, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                {
                    var val = p.GetValue(source);
                    if (val == null) return null;
                    if (val is DateTime dt) return dt;
                    if (DateTime.TryParse(val.ToString(), out var parsed)) return parsed;
                }
            }
            return null;
        }

        private static IEnumerable<T>? GetCollection<T>(object source, params string[] preferredNames)
        {
            // 1) Intenta por nombres conocidos
            foreach (var n in preferredNames)
            {
                var p = source.GetType().GetProperty(n, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                {
                    var list = CoerceEnumerable<T>(p.GetValue(source));
                    if (list != null) return list;
                }
            }
            // 2) Último recurso: busca cualquier propiedad IEnumerable<T>
            foreach (var p in source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var list = CoerceEnumerable<T>(p.GetValue(source));
                if (list != null) return list;
            }
            return null;
        }

        private static IEnumerable<T>? CoerceEnumerable<T>(object? value)
        {
            if (value is null) return null;
            if (value is IEnumerable en)
            {
                var result = new List<T>();
                foreach (var item in en)
                {
                    if (item is T t) result.Add(t);
                    else return null; // mezcla de tipos -> no nos sirve
                }
                return result;
            }
            return null;
        }
    }
}
