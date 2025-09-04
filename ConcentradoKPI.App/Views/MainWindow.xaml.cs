using System.Windows;
using System.Windows.Controls;
using ConcentradoKPI.App.ViewModels;
using ConcentradoKPI.App.Models;
using System.Windows.Input;


namespace ConcentradoKPI.App.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
