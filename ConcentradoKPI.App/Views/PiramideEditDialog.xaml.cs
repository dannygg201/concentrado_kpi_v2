using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using ConcentradoKPI.App.Models;

namespace ConcentradoKPI.App.Views
{
    public partial class PiramideEditDialog : Window
    {
        // ViewModel simple para DataContext del diálogo
        public class Vm
        {
            public PiramideValues Values { get; set; } = new();

            // Nuevo: campo de trabajo como fecha (nullable) para el DatePicker
            public DateTime? LastRecordDate { get; set; }
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

            // Clonamos valores actuales y mapeamos LastRecord (string) -> DateTime?
            var vm = new Vm { Values = current.Clone() };

            if (!string.IsNullOrWhiteSpace(vm.Values.LastRecord))
            {
                // Intentamos parsear con formatos comunes (yyyy-MM-dd preferente)
                if (DateTime.TryParseExact(vm.Values.LastRecord,
                                           new[] { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "yyyy/MM/dd" },
                                           CultureInfo.InvariantCulture,
                                           DateTimeStyles.None,
                                           out var parsed))
                {
                    vm.LastRecordDate = parsed;
                }
                else if (DateTime.TryParse(vm.Values.LastRecord, out var parsed2))
                {
                    vm.LastRecordDate = parsed2;
                }
            }

            DataContext = vm;
        }

        // Solo permite dígitos (para los numéricos)
        private static readonly Regex _digits = new(@"^\d+$");
        private void NumericOnly(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Vm vm)
            {
                // Normaliza rangos
                if (vm.Values.AvanceProgramaPct < 0) vm.Values.AvanceProgramaPct = 0;
                if (vm.Values.AvanceProgramaPct > 100) vm.Values.AvanceProgramaPct = 100;

                // Mapear DatePicker -> string (o vacío si no eligieron fecha)
                vm.Values.LastRecord = vm.LastRecordDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

                Result = vm.Values;
                DialogResult = true; // OK
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Cancel
        }
    }
}
