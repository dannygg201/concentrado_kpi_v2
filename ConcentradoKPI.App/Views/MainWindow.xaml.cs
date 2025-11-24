using System.Windows;
using System.Linq;
using System.Windows.Controls;
using ConcentradoKPI.App.ViewModels;
using ConcentradoKPI.App.Models;
using System.Windows.Input;
using System.Collections.Generic;
using System;
using ConcentradoKPI.App.Views;
using ConcentradoKPI.App.Views.Pages;
using System.ComponentModel;

namespace ConcentradoKPI.App.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            PersonalVigenteViewModel.PersonalVigenteUpdated -= OnPersonalVigenteUpdated;
            PersonalVigenteViewModel.PersonalVigenteUpdated += OnPersonalVigenteUpdated;
        }

        // Registro de ventanas abiertas
        private readonly Dictionary<string, ShellWindow> _openWindows = new();
        private static string MakeKey(Company c, Project p, WeekData w)
            => $"{c.Name}|{p.Name}|{w.WeekNumber}";

        private static void BringToFront(Window w)
        {
            if (w.WindowState == WindowState.Minimized) w.WindowState = WindowState.Normal;
            w.Topmost = true; w.Topmost = false;
            w.Activate(); w.Focus();
        }

        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.OpenPersonalRequested -= Vm_OpenPersonalRequested;
                vm.OpenPersonalRequested += Vm_OpenPersonalRequested;

                // 👇 Nos suscribimos a cambios del VM
                vm.PropertyChanged -= Vm_PropertyChanged;
                vm.PropertyChanged += Vm_PropertyChanged;

                // 🔹 Si NO hay selección pero sí hay datos, toma la primera compañía/proyecto/semana
                if (vm.SelectedCompany == null || vm.SelectedProject == null || vm.SelectedWeek == null)
                {
                    var firstCompany = vm.Companies.FirstOrDefault();
                    var firstProject = firstCompany?.Projects.FirstOrDefault();
                    var firstWeek = firstProject?.Weeks.FirstOrDefault();

                    if (firstCompany != null && firstProject != null && firstWeek != null)
                    {
                        vm.SelectedCompany = firstCompany;
                        vm.SelectedProject = firstProject;
                        vm.SelectedWeek = firstWeek;
                    }
                }

                // 🔹 Si ya hay selección, carga la vista previa
                if (vm.SelectedCompany != null && vm.SelectedProject != null && vm.SelectedWeek != null)
                    LoadPersonalVigentePreview(vm.SelectedCompany, vm.SelectedProject, vm.SelectedWeek);
            }
        }

        private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not MainViewModel vm) return;

            // Solo reaccionamos a cambios de selección
            if (e.PropertyName == nameof(MainViewModel.SelectedWeek) ||
                e.PropertyName == nameof(MainViewModel.SelectedProject) ||
                e.PropertyName == nameof(MainViewModel.SelectedCompany))
            {
                if (vm.SelectedCompany != null && vm.SelectedProject != null && vm.SelectedWeek != null)
                {
                    // 🔎 Recarga la vista previa cuando haya contexto completo
                    LoadPersonalVigentePreview(vm.SelectedCompany, vm.SelectedProject, vm.SelectedWeek);
                }
                else
                {
                    // Si se limpia la selección, limpiamos la vista previa
                    ClearPreview();
                }
            }
        }


        // ===== Preview helpers =====
        private void LoadPersonalVigentePreview(Company c, Project p, WeekData w)
        {
            var view = new PersonalVigenteView(c, p, w)
            {
                IsHitTestVisible = false,
                Focusable = false
            };

            PreviewHost.Content = view;
            w?.Live?.NotifyAll();
        }


        private void ClearPreview()
        {
            PreviewHost.Content = null;
        }

        // Abre Shell
        private void Vm_OpenPersonalRequested(Company company, Project project, WeekData week)
        {
            var key = MakeKey(company, project, week);

            if (_openWindows.TryGetValue(key, out var existing) && existing.IsLoaded)
            {
                BringToFront(existing);
                return;
            }

            var shell = new ShellWindow(company, project, week, (MainViewModel)DataContext)
            {
                Owner = this,
                ShowInTaskbar = true,
                Title = $"Concentrado KPI - {company.Name} · {project.Name} · Semana {week.WeekNumber}"
            };

            shell.Closed += (_, __) => _openWindows.Remove(key);

            _openWindows[key] = shell;
            shell.Show();
            BringToFront(shell);
        }

        // Context menus: mantienen selección
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
                    ClearPreview();
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

                    // 🔹 Igual que en el TreeView: si hay semanas, toma la primera y muestra preview
                    var firstWeek = project.Weeks.FirstOrDefault();
                    if (firstWeek != null)
                    {
                        vm.SelectedWeek = firstWeek;
                        LoadPersonalVigentePreview(vm.SelectedCompany!, vm.SelectedProject!, vm.SelectedWeek!);
                    }
                    else
                    {
                        vm.SelectedWeek = null;
                        ClearPreview();
                    }

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

                    // 🔎 Actualiza preview
                    if (vm.SelectedCompany != null && vm.SelectedProject != null && vm.SelectedWeek != null)
                        LoadPersonalVigentePreview(vm.SelectedCompany, vm.SelectedProject, vm.SelectedWeek);
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
                    ClearPreview();
                    break;

                case Project p:
                    vm.SelectedProject = p;
                    foreach (var comp in vm.Companies)
                        if (comp.Projects.Contains(p)) { vm.SelectedCompany = comp; break; }

                    // 🔹 Si el proyecto tiene semanas, auto-selecciona la primera y muestra la preview
                    var firstWeek = p.Weeks.FirstOrDefault();
                    if (firstWeek != null)
                    {
                        vm.SelectedWeek = firstWeek;
                        LoadPersonalVigentePreview(vm.SelectedCompany!, vm.SelectedProject!, vm.SelectedWeek!);
                    }
                    else
                    {
                        vm.SelectedWeek = null;
                        ClearPreview();
                    }
                    break;

                case WeekData w:
                    vm.SelectedWeek = w;
                    foreach (var comp in vm.Companies)
                        foreach (var proj in comp.Projects)
                            if (proj.Weeks.Contains(w))
                            { vm.SelectedProject = proj; vm.SelectedCompany = comp; break; }

                    // 🔎 Carga la vista previa
                    if (vm.SelectedCompany != null && vm.SelectedProject != null && vm.SelectedWeek != null)
                        LoadPersonalVigentePreview(vm.SelectedCompany, vm.SelectedProject, vm.SelectedWeek);
                    break;

                default:
                    ClearPreview();
                    break;
            }
        }
        private void OnPersonalVigenteUpdated(WeekData updatedWeek)
        {
            // Nos regresamos al hilo de UI por si viene de otro contexto
            Dispatcher.Invoke(() =>
            {
                if (DataContext is not MainViewModel vm) return;

                // Solo recargamos si la semana actual seleccionada es justo la que se actualizó
                if (vm.SelectedCompany == null || vm.SelectedProject == null || vm.SelectedWeek == null)
                    return;

                if (!ReferenceEquals(vm.SelectedWeek, updatedWeek))
                    return;

                // 🔃 Re-crear la vista previa con los datos ya guardados
                LoadPersonalVigentePreview(vm.SelectedCompany, vm.SelectedProject, vm.SelectedWeek);
            });
        }

    }
}
