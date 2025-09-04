using System.Collections.Generic;
using System.Windows;
using ConcentradoKPI.App.Models;

namespace ConcentradoKPI.App.Views
{
    public partial class SelectCompanyDialog : Window
    {
        public Company? SelectedCompany { get; private set; }

        public SelectCompanyDialog(IList<Company> companies, Company? preselect)
        {
            InitializeComponent();
            Cmb.ItemsSource = companies;
            if (preselect != null) Cmb.SelectedItem = preselect;
            else if (companies.Count > 0) Cmb.SelectedIndex = 0;
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            SelectedCompany = Cmb.SelectedItem as Company;
            if (SelectedCompany == null)
            {
                MessageBox.Show("Selecciona una compañía.", "Guardar como",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            DialogResult = true;
        }
    }
}
