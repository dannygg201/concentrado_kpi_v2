using ConcentradoKPI.App.Models;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

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
            vm.Values.Avance = CalcularAvanceCondiciones(vm.Values.Detectadas, vm.Values.Corregidas);
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

                // 🔹 Recalcular avance condiciones SOLO con Detectadas/Corregidas
                vm.Values.Avance = CalcularAvanceCondiciones(vm.Values.Detectadas, vm.Values.Corregidas);

                // Mapear DatePicker -> string
                vm.Values.LastRecord = vm.LastRecordDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

                Result = vm.Values;
                DialogResult = true; // OK
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Cancel
        }
        private static int CalcularAvanceCondiciones(int detectadas, int corregidas)
        {
            if (detectadas <= 0)
                return 0;

            double ratio = (double)corregidas / detectadas;
            if (ratio < 0) ratio = 0;
            if (ratio > 1) ratio = 1;

            // porcentaje entero 0–100
            return (int)Math.Round(ratio * 100, MidpointRounding.AwayFromZero);
        }
        private void DetectadasOCorregidas_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is Vm vm)
            {
                // Recalcular avance en el modelo
                vm.Values.Avance = CalcularAvanceCondiciones(vm.Values.Detectadas, vm.Values.Corregidas);

                // Refrescar el TextBox de Avance inmediatamente
                TxtAvance.Text = vm.Values.Avance.ToString(CultureInfo.InvariantCulture);
            }
        }

    }
}
