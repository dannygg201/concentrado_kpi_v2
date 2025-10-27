using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using ConcentradoKPI.App.Models;

namespace ConcentradoKPI.App.Views
{
    public partial class PiramideEditDialog : Window
    {
        // ViewModel simple para DataContext del diálogo
        public class Vm
        {
            public PiramideValues Values { get; set; } = new();
        }

        public PiramideEditDialog()
        {
            InitializeComponent();
            Loaded += (_, __) =>
            {
                WindowState = WindowState.Normal;
                Left = Owner != null ? Owner.Left + (Owner.Width - Width) / 2 : Left;
                Top = Owner != null ? Owner.Top + (Owner.Height - Height) / 2 : Top;
            };
        }

        public PiramideValues Result { get; private set; } = new();

        public PiramideEditDialog(PiramideValues current)
        {
            InitializeComponent();
            DataContext = new Vm { Values = current.Clone() }; // Trabajamos con copia
        }

        // Solo permite dígitos (se permiten Backspace/Delete a nivel de control)
        private static readonly Regex _digits = new(@"^\d+$");
        private void NumericOnly(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]+$");
        }


        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Vm vm)
            {
                // Normaliza algunos rangos (ajusta si necesitas más reglas)
                if (vm.Values.AvanceProgramaPct < 0) vm.Values.AvanceProgramaPct = 0;
                if (vm.Values.AvanceProgramaPct > 100) vm.Values.AvanceProgramaPct = 100;

                Result = vm.Values;
                DialogResult = true; // cierra el diálogo devolviendo OK
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;  // cierra el diálogo sin aplicar cambios
        }


    }
}
