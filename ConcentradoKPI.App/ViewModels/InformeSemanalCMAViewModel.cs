using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.Services; // LastEspecialidadStore

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

        // 🔹 NUEVOS
        private int _corr; public int Corregidas { get => _corr; set { if (Set(ref _corr, value)) RaiseCalc(); } }
        private int _det; public int Detectadas { get => _det; set { if (Set(ref _det, value)) RaiseCalc(); } }

        public int TotalSemanal =>
            Incidentes + PrecursoresSifComportamiento + PrecursoresSifCondicion + ActosInseguros + ActosSeguros;

        public double PorcentajeAvance =>
            (ActosSeguros + ActosInseguros) == 0 ? 1.0
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
        // Top info
        public Company Company { get; }
        public Project Project { get; }
        public WeekData Week { get; }

        public string EncabezadoDescripcion { get; }

        // Items
        public ObservableCollection<ContratistaRow> Cards { get; } = new();

        public ContratistaRow Editable { get; } = new();
        public ContratistaRow TotalesFila { get; } = new() { IsTotal = true, Nombre = "Totales" };

        private void PullFromLiveToEditable()
        {
            if (Week?.Live == null) return;
            Editable.Colaboradores = Week.Live.ColaboradoresTotal;
            Editable.HorasTrabajadas = Week.Live.HorasTrabajadasTotal;
            Editable.TecnicosSeguridad = Week.Live.TecnicosSeguridadTotal;
        }

        private void HydrateFromSavedWeekly(InformeSemanalCmaDocument? d)
        {
            if (d == null) return;

            Editable.Nombre = string.IsNullOrWhiteSpace(d.Nombre) ? Editable.Nombre : d.Nombre;
            if (!string.IsNullOrWhiteSpace(d.Especialidad))
                Editable.Especialidad = d.Especialidad;

            // No tocar: TecnicosSeguridad, Colaboradores, HorasTrabajadas (vienen de Live)

            Editable.LTI = d.LTI;
            Editable.MDI = d.MDI;
            Editable.MTI = d.MTI;
            Editable.TRI = d.TRI;
            Editable.FAI = d.FAI;

            Editable.Incidentes = d.Incidentes;

            Editable.PrecursoresSifComportamiento = d.PrecursoresSifComportamiento;
            Editable.PrecursoresSifCondicion = d.PrecursoresSifCondicion;

            Editable.ActosSeguros = d.ActosSeguros;
            Editable.ActosInseguros = d.ActosInseguros;

            // 🔹 Nuevos
            Editable.Corregidas = d.Corregidas;
            Editable.Detectadas = d.Detectadas;
        }


        public InformeSemanalCMAViewModel(
            Company c,
            Project p,
            WeekData w,
            string nombreInicial = "",
            string especialidadInicial = "")
        {
            Company = c;
            Project = p;
            Week = w;

            EncabezadoDescripcion = $"Semana {w.WeekNumber}  |  {p.Name}";

            Editable.Nombre = string.IsNullOrWhiteSpace(nombreInicial) ? c.Name : nombreInicial;
            Editable.Especialidad = especialidadInicial;

            // 1) Live primero
            PullFromLiveToEditable();

            // 2) Ya no jalamos de Pirámide (se queda en blanco si no hay informe guardado)
            HydrateFromSavedWeekly(Week?.InformeSemanalCma);

            // 3) Última especialidad usada
            if (string.IsNullOrWhiteSpace(Editable.Especialidad))
            {
                var last = LastEspecialidadStore.Get(Company?.Name ?? "", Project?.Name ?? "");
                if (!string.IsNullOrWhiteSpace(last)) Editable.Especialidad = last!;
            }

            // 4) Suscripción Live (solo esos 3)
            if (Week?.Live != null)
            {
                Week.Live.PropertyChanged += (_, e) =>
                {
                    if (string.IsNullOrEmpty(e.PropertyName) ||
                        e.PropertyName == nameof(LiveMetrics.ColaboradoresTotal) ||
                        e.PropertyName == nameof(LiveMetrics.HorasTrabajadasTotal) ||
                        e.PropertyName == nameof(LiveMetrics.TecnicosSeguridadTotal))
                    {
                        System.Windows.Application.Current.Dispatcher.InvokeAsync(PullFromLiveToEditable);
                    }
                };
            }

            // 5) Totales espejo
            Editable.PropertyChanged += (_, __) => RecalcularTotales();
            RecalcularTotales();

            // 6) Items
            Cards.Add(Editable);
            Cards.Add(TotalesFila);
        }

        // Diseño
        public InformeSemanalCMAViewModel(string semana, string proyecto, string nombreInicial = "", string especialidadInicial = "")
        {
            Company = new Company { Name = string.IsNullOrWhiteSpace(nombreInicial) ? "Demo Co." : nombreInicial };
            Project = new Project { Name = string.IsNullOrWhiteSpace(proyecto) ? "Proyecto Demo" : proyecto };
            Week = new WeekData { WeekNumber = int.TryParse(semana, out var n) ? n : 1 };

            EncabezadoDescripcion = $"Semana {semana}  |  {proyecto}";

            Editable.Nombre = string.IsNullOrWhiteSpace(nombreInicial) ? Company.Name : nombreInicial;
            Editable.Especialidad = especialidadInicial;

            Editable.PropertyChanged += (_, __) => RecalcularTotales();
            RecalcularTotales();

            Cards.Add(Editable);
            Cards.Add(TotalesFila);
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

            // 🔹 Nuevos totales
            TotalesFila.Corregidas = Editable.Corregidas;
            TotalesFila.Detectadas = Editable.Detectadas;

            TotalesFila.OnPropertyChanged(nameof(ContratistaRow.TotalSemanal));
            TotalesFila.OnPropertyChanged(nameof(ContratistaRow.PorcentajeAvance));
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
