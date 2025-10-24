using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;   // <— agrega esto

namespace ConcentradoKPI.App.ViewModels
{
    public class ContratistaRow : INotifyPropertyChanged
    {
        protected bool Set<T>(ref T f, T v, [CallerMemberName] string? n = null)
        { if (Equals(f, v)) return false; f = v; OnPropertyChanged(n); return true; }

        private string _nombre = ""; public string Nombre { get => _nombre; set => Set(ref _nombre, value); }
        private string _especialidad = ""; public string Especialidad { get => _especialidad; set => Set(ref _especialidad, value); }

        private int _tec; public int TecnicosSeguridad { get => _tec; set { if (Set(ref _tec, value)) RaiseCalc(); } }
        private int _col; public int Colaboradores { get => _col; set { if (Set(ref _col, value)) RaiseCalc(); } }
        private int _hrs; public int HorasTrabajadas { get => _hrs; set { if (Set(ref _hrs, value)) RaiseCalc(); } }

        private int _lti; public int LTI { get => _lti; set { if (Set(ref _lti, value)) RaiseCalc(); } }
        private int _mdi; public int MDI { get => _mdi; set { if (Set(ref _mdi, value)) RaiseCalc(); } }
        private int _mti; public int MTI { get => _mti; set { if (Set(ref _mti, value)) RaiseCalc(); } }
        private int _tri; public int TRI { get => _tri; set { if (Set(ref _tri, value)) RaiseCalc(); } }
        private int _fai; public int FAI { get => _fai; set { if (Set(ref _fai, value)) RaiseCalc(); } }

        private int _inc; public int Incidentes { get => _inc; set { if (Set(ref _inc, value)) RaiseCalc(); } }
        private int _psC; public int PrecursoresSifComportamiento { get => _psC; set { if (Set(ref _psC, value)) RaiseCalc(); } }
        private int _psK; public int PrecursoresSifCondicion { get => _psK; set { if (Set(ref _psK, value)) RaiseCalc(); } }
        private int _as; public int ActosSeguros { get => _as; set { if (Set(ref _as, value)) RaiseCalc(); } }
        private int _ai; public int ActosInseguros { get => _ai; set { if (Set(ref _ai, value)) RaiseCalc(); } }

        public int TotalSemanal =>
            Incidentes + PrecursoresSifComportamiento + PrecursoresSifCondicion + ActosInseguros + ActosSeguros;

        public double PorcentajeAvance
            => (ActosSeguros + ActosInseguros) == 0 ? 1.0
               : Math.Clamp((double)ActosSeguros / (ActosSeguros + ActosInseguros), 0, 1);

        public bool IsTotal { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string? n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        private void RaiseCalc()
        {
            OnPropertyChanged(nameof(TotalSemanal));
            OnPropertyChanged(nameof(PorcentajeAvance));
        }
    }

    public class InformeSemanalCMAViewModel : INotifyPropertyChanged
    {
        public string EncabezadoDescripcion { get; }

        // Colección para el ItemsControl (Opción 1)
        public ObservableCollection<ContratistaRow> Cards { get; } = new();

        // Seguimos usando estos para lógica interna (si quieres)
        public ContratistaRow Editable { get; } = new();
        public ContratistaRow TotalesFila { get; } = new() { IsTotal = true, Nombre = "Totales" };

        public InformeSemanalCMAViewModel(
            string semana,
            string proyecto,
            string nombreInicial = "",
            string especialidadInicial = "")
        {
            EncabezadoDescripcion = $"Semana {semana}  |  {proyecto}";

            // Inicial editable
            Editable.Nombre = nombreInicial;
            Editable.Especialidad = especialidadInicial;

            // Ligar cambios de Editable a Totales
            Editable.PropertyChanged += (_, __) => RecalcularTotales();
            RecalcularTotales();

            // IMPORTANTÍSIMO: llenar la colección que usa el ItemsControl
            Cards.Add(Editable);     // tarjeta editable
            Cards.Add(TotalesFila);  // tarjeta de totales
        }

        private void RecalcularTotales()
        {
            TotalesFila.TecnicosSeguridad = Editable.TecnicosSeguridad;
            TotalesFila.Colaboradores = Editable.Colaboradores;
            TotalesFila.HorasTrabajadas = Editable.HorasTrabajadas;
            TotalesFila.LTI = Editable.LTI;
            TotalesFila.MDI = Editable.MDI;
            TotalesFila.MTI = Editable.MTI;
            TotalesFila.TRI = Editable.TRI;
            TotalesFila.FAI = Editable.FAI;
            TotalesFila.Incidentes = Editable.Incidentes;
            TotalesFila.PrecursoresSifComportamiento = Editable.PrecursoresSifComportamiento;
            TotalesFila.PrecursoresSifCondicion = Editable.PrecursoresSifCondicion;
            TotalesFila.ActosSeguros = Editable.ActosSeguros;
            TotalesFila.ActosInseguros = Editable.ActosInseguros;

            TotalesFila.OnPropertyChanged(nameof(ContratistaRow.TotalSemanal));
            TotalesFila.OnPropertyChanged(nameof(ContratistaRow.PorcentajeAvance));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
