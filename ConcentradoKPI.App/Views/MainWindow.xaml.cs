using System.Windows;
using System.Linq;
using System.Windows.Controls;
using ConcentradoKPI.App.ViewModels;
using ConcentradoKPI.App.Models;
using System.Windows.Input;
using System.Collections.Generic;
using System;
using ConcentradoKPI.App.Views;


namespace ConcentradoKPI.App.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

        }
        // ===== Registro de ventanas abiertas por clave (empresa|proyecto|semana) =====
        private readonly Dictionary<string, PersonalVigenteWindow> _openWindows = new();
       
        private static string MakeKey(Company c, Project p, WeekData w)
     => $"{c.Name}|{p.Name}|{w.WeekNumber}";


        private static void BringToFront(Window w)
        {
            if (w.WindowState == WindowState.Minimized) w.WindowState = WindowState.Normal;
            // Truco para forzar foco
            w.Topmost = true; w.Topmost = false;
            w.Activate(); w.Focus();
        }

        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                // Evita doble suscripción
                vm.OpenPersonalRequested -= Vm_OpenPersonalRequested;
                vm.OpenPersonalRequested += Vm_OpenPersonalRequested;
            }
        }
        // 🔹 Abre la nueva ventana de personal vigente
        // 🔹 Abre la nueva ventana de personal vigente
        private void Vm_OpenPersonalRequested(Company company, Project project, WeekData week)
        {
            var key = MakeKey(company, project, week);

            // Si ya está abierta, solo traemos al frente
            if (_openWindows.TryGetValue(key, out var existing) && existing.IsLoaded)
            {
                BringToFront(existing);
                return;
            }

            // Crea una nueva ventana modeless
            var vm = new PersonalVigenteViewModel(company, project, week);

            // ⬇️ AQUI EL CAMBIO: pásale los 3 argumentos al constructor
            var win = new PersonalVigenteWindow(company, project, week)
            {
                Owner = this,                 // o null si la quieres totalmente independiente
                ShowInTaskbar = true,
                DataContext = vm,
                Title = $"Personal vigente - {company.Name} · {project.Name} · Semana {week.WeekNumber}"
            };

            // Guarda en el registro
            _openWindows[key] = win;

            // Limpieza al cerrar
            win.Closed += (_, __) =>
            {
                _openWindows.Remove(key);
                if (vm is IDisposable d) d.Dispose();
            };

            win.Show();
            BringToFront(win);
        }


        private void CompanyMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel vm) return;
            if (sender is ContextMenu cm)
            {
                var company = (cm.PlacementTarget as FrameworkElement)?.DataContext as Company;
                if (company != null)
                {
                    vm.SelectedCompany = company;
                    vm.SelectedProject = null;
                    vm.SelectedWeek = null;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private void ProjectMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel vm) return;
            if (sender is ContextMenu cm)
            {
                var project = (cm.PlacementTarget as FrameworkElement)?.DataContext as Project;
                if (project != null)
                {
                    vm.SelectedProject = project;
                    foreach (var c in vm.Companies)
                        if (c.Projects.Contains(project)) { vm.SelectedCompany = c; break; }
                    vm.SelectedWeek = null;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private void WeekMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel vm) return;
            if (sender is ContextMenu cm)
            {
                var week = (cm.PlacementTarget as FrameworkElement)?.DataContext as WeekData;
                if (week != null)
                {
                    vm.SelectedWeek = week;
                    foreach (var c in vm.Companies)
                        foreach (var p in c.Projects)
                            if (p.Weeks.Contains(week)) { vm.SelectedProject = p; vm.SelectedCompany = c; break; }
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }



        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                item.IsSelected = true;
                item.Focus();
            }
        }


        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is not MainViewModel vm) return;

            switch (e.NewValue)
            {
                case Company c:
                    vm.SelectedCompany = c;
                    vm.SelectedProject = null;
                    vm.SelectedWeek = null;
                    break;

                case Project p:
                    vm.SelectedProject = p;
                    // actualizar compañía según el árbol
                    foreach (var comp in vm.Companies)
                        if (comp.Projects.Contains(p)) { vm.SelectedCompany = comp; break; }
                    vm.SelectedWeek = null;
                    break;

                case WeekData w:
                    vm.SelectedWeek = w;
                    foreach (var comp in vm.Companies)
                        foreach (var proj in comp.Projects)
                            if (proj.Weeks.Contains(w))
                            { vm.SelectedProject = proj; vm.SelectedCompany = comp; break; }
                    break;
            }
        }
    }
}
