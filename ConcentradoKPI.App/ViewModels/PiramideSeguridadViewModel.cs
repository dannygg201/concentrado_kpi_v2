using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using ConcentradoKPI.App.Models;

namespace ConcentradoKPI.App.ViewModels
{
    public class PiramideSeguridadViewModel : INotifyPropertyChanged, IDisposable
    {
        public Company Company { get; }
        public Project Project { get; }
        public WeekData Week { get; }

        // ====== Handler para desuscribirnos de Live ======
        private PropertyChangedEventHandler? _liveHandler;

        // ====== SYNC con Week.Live ======
        private void PullFromLive()
        {
            if (Week?.Live == null) return;

            Colaboradores = Week.Live.ColaboradoresTotal.ToString(CultureInfo.InvariantCulture);
            HorasTrabajadas = Week.Live.HorasTrabajadasTotal.ToString(CultureInfo.InvariantCulture);
            TecnicosSeguridad = Week.Live.TecnicosSeguridadTotal.ToString(CultureInfo.InvariantCulture);
        }

        public PiramideSeguridadViewModel(Company c, Project p, WeekData w)
        {
            Company = c;
            Project = p;
            Week = w;

            // Si hay Live, úsalo como fuente de verdad y escucha cambios
            if (Week?.Live != null)
            {
                PullFromLive();

                _liveHandler = (_, e) =>
                {
                    if (string.IsNullOrEmpty(e.PropertyName) ||
                        e.PropertyName == nameof(LiveMetrics.ColaboradoresTotal) ||
                        e.PropertyName == nameof(LiveMetrics.HorasTrabajadasTotal) ||
                        e.PropertyName == nameof(LiveMetrics.TecnicosSeguridadTotal))
                    {
                        System.Windows.Application.Current.Dispatcher.InvokeAsync(PullFromLive);
                    }
                };
                Week.Live.PropertyChanged += _liveHandler;
            }
            else
            {
                // Modo offline (rehidratar desde Week.PersonalVigente)
                RecalculateFromWeek();
            }

            // ===== Valores iniciales para lo demás de la vista =====
            MTH = "MTH";
            YTD = DateTime.Now.Year.ToString();

            LTI = "0"; MDI = "0"; MTI = "0"; FAI = "0";
            IncidentesSinLesion = "0";
            Potenciales = "0"; Precursores = "0";
            Seguros = "0"; Inseguros = "0";
            CondicionesDetectadas = "0"; CondicionesCorregidas = "0"; AvanceCondiciones = "0%";
            AvanceProgramaPct = "0";

            if (string.IsNullOrWhiteSpace(Companias)) Companias = "0";
            if (string.IsNullOrWhiteSpace(TecnicosSeguridad)) TecnicosSeguridad = "0";
            if (string.IsNullOrWhiteSpace(WithoutLTIs)) WithoutLTIs = "0 días";
            if (string.IsNullOrWhiteSpace(LastRecord)) LastRecord = "2025-01-01";

            FAI1 = "0"; FAI2 = "0"; FAI3 = "0";
            MTI1 = "0"; MTI2 = "0"; MTI3 = "0";
            MDI1 = "0"; MDI2 = "0"; MDI3 = "0";
            LTI1 = "0"; LTI2 = "0"; LTI3 = "0";
            IncidentesSinLesion1 = "0"; IncidentesSinLesion2 = "0";
            Precursores1 = "0"; Precursores2 = "0"; Precursores3 = "0";
            Efectividad = "0";
        }

        /// <summary>
        /// Relee Week.PersonalVigente para calcular Colaboradores, HorasTrabajadas y Técnicos de Seguridad.
        /// Úsalo solo si NO tienes Live o cuando hidrates desde archivo.
        /// </summary>
        /// 
        private void RecalculateAvanceCondiciones()
        {
            // Parsear detectadas
            if (!int.TryParse(CondicionesDetectadas, NumberStyles.Any, CultureInfo.InvariantCulture, out var detectadas)
                || detectadas <= 0)
            {
                AvanceCondiciones = "0%";
                return;
            }

            // Parsear corregidas
            if (!int.TryParse(CondicionesCorregidas, NumberStyles.Any, CultureInfo.InvariantCulture, out var corregidas))
                corregidas = 0;

            if (corregidas < 0) corregidas = 0;
            if (corregidas > detectadas) corregidas = detectadas;

            var pct = (int)Math.Round((double)corregidas * 100.0 / detectadas);
            AvanceCondiciones = $"{pct}%";
        }
        public void RecalculateFromWeek()
        {
            var lista = Week?.PersonalVigenteDocument?.Personal ?? Enumerable.Empty<PersonRow>();

            var colaboradores = lista.Count();
            var horas = lista.Sum(r => r?.HHSemana ?? 0);
            var tecnicosSeg = lista.Count(r => r?.EsTecnicoSeguridad == true);

            Colaboradores = colaboradores.ToString(CultureInfo.InvariantCulture);
            HorasTrabajadas = horas.ToString(CultureInfo.InvariantCulture);
            TecnicosSeguridad = tecnicosSeg.ToString(CultureInfo.InvariantCulture);
        }

        // ===== Cabeceras =====
        string _MTH = ""; public string MTH { get => _MTH; set { _MTH = value; OnPropertyChanged(); } }
        string _YTD = ""; public string YTD { get => _YTD; set { _YTD = value; OnPropertyChanged(); } }

        // ===== Pirámide =====
        string _LTI = "0"; public string LTI { get => _LTI; set { _LTI = value; OnPropertyChanged(); } }
        string _MDI = "0"; public string MDI { get => _MDI; set { _MDI = value; OnPropertyChanged(); } }
        string _MTI = "0"; public string MTI { get => _MTI; set { _MTI = value; OnPropertyChanged(); } }
        string _FAI = "0"; public string FAI { get => _FAI; set { _FAI = value; OnPropertyChanged(); } }
        string _Inc = "0"; public string IncidentesSinLesion { get => _Inc; set { _Inc = value; OnPropertyChanged(); } }
        string _Pot = "0"; public string Potenciales { get => _Pot; set { _Pot = value; OnPropertyChanged(); } }
        string _Pre = "0"; public string Precursores { get => _Pre; set { _Pre = value; OnPropertyChanged(); } }
        string _Seg = "0"; public string Seguros { get => _Seg; set { _Seg = value; OnPropertyChanged(); } }
        string _Ins = "0"; public string Inseguros { get => _Ins; set { _Ins = value; OnPropertyChanged(); } }
        string _CD = "0";
        public string CondicionesDetectadas
        {
            get => _CD;
            set
            {
                if (_CD == value) return;
                _CD = value;
                OnPropertyChanged();
                RecalculateAvanceCondiciones();
            }
        }
        string _CC = "0";
        public string CondicionesCorregidas
        {
            get => _CC;
            set
            {
                if (_CC == value) return;
                _CC = value;
                OnPropertyChanged();
                RecalculateAvanceCondiciones();
            }
        }
        string _AvC = "0%";
        public string AvanceCondiciones
        {
            get => _AvC;
            private set   // ← importante: que no lo modifiquen desde fuera
            {
                if (_AvC == value) return;
                _AvC = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Avance)); // alias
            }
        }
        string _AvP = "0"; public string AvanceProgramaPct { get => _AvP; set { _AvP = value; OnPropertyChanged(); } }

        // ===== Laterales =====
        string _Comp = "0"; public string Companias { get => _Comp; set { _Comp = value; OnPropertyChanged(); } }

        string _Col = "0";
        public string Colaboradores { get => _Col; set { if (_Col == value) return; _Col = value; OnPropertyChanged(); } }

        string _Tec = "0";
        public string TecnicosSeguridad { get => _Tec; set { if (_Tec == value) return; _Tec = value; OnPropertyChanged(); } }

        string _Hrs = "0";
        public string HorasTrabajadas { get => _Hrs; set { if (_Hrs == value) return; _Hrs = value; OnPropertyChanged(); } }

        string _W = ""; public string WithoutLTIs { get => _W; set { _W = value; OnPropertyChanged(); } }
        string _LR = ""; public string LastRecord { get => _LR; set { _LR = value; OnPropertyChanged(); } }

        // ===== Aliases para XAML =====
        public string Detectadas
        {
            get => CondicionesDetectadas;
            set => CondicionesDetectadas = value;
        }
        public string Corregidas
        {
            get => CondicionesCorregidas;
            set => CondicionesCorregidas = value;
        }
        public string Avance => AvanceCondiciones;
        // ===== Incidentes sin lesión (2 cuadros) =====
        string _Inc1 = "0"; public string IncidentesSinLesion1 { get => _Inc1; set { _Inc1 = value; OnPropertyChanged(); } }
        string _Inc2 = "0"; public string IncidentesSinLesion2 { get => _Inc2; set { _Inc2 = value; OnPropertyChanged(); } }

        // ===== Precursores (3 cuadros) =====
        string _Pre1 = "0"; public string Precursores1 { get => _Pre1; set { _Pre1 = value; OnPropertyChanged(); } }
        string _Pre2 = "0"; public string Precursores2 { get => _Pre2; set { _Pre2 = value; OnPropertyChanged(); } }
        string _Pre3 = "0"; public string Precursores3 { get => _Pre3; set { _Pre3 = value; OnPropertyChanged(); } }

        // ===== FAI / MTI / MDI / LTI (3 por nivel) =====
        string _FAI1 = "0"; public string FAI1 { get => _FAI1; set { _FAI1 = value; OnPropertyChanged(); } }
        string _FAI2 = "0"; public string FAI2 { get => _FAI2; set { _FAI2 = value; OnPropertyChanged(); } }
        string _FAI3 = "0"; public string FAI3 { get => _FAI3; set { _FAI3 = value; OnPropertyChanged(); } }

        string _MTI1 = "0"; public string MTI1 { get => _MTI1; set { _MTI1 = value; OnPropertyChanged(); } }
        string _MTI2 = "0"; public string MTI2 { get => _MTI2; set { _MTI2 = value; OnPropertyChanged(); } }
        string _MTI3 = "0"; public string MTI3 { get => _MTI3; set { _MTI3 = value; OnPropertyChanged(); } }

        string _MDI1 = "0"; public string MDI1 { get => _MDI1; set { _MDI1 = value; OnPropertyChanged(); } }
        string _MDI2 = "0"; public string MDI2 { get => _MDI2; set { _MDI2 = value; OnPropertyChanged(); } }
        string _MDI3 = "0"; public string MDI3 { get => _MDI3; set { _MDI3 = value; OnPropertyChanged(); } }

        string _LTI1 = "0"; public string LTI1 { get => _LTI1; set { _LTI1 = value; OnPropertyChanged(); } }
        string _LTI2 = "0"; public string LTI2 { get => _LTI2; set { _LTI2 = value; OnPropertyChanged(); } }
        string _LTI3 = "0"; public string LTI3 { get => _LTI3; set { _LTI3 = value; OnPropertyChanged(); } }

        // ===== Efectividad =====
        string _Efect = "0"; public string Efectividad { get => _Efect; set { _Efect = value; OnPropertyChanged(); } }

        // Territorios (rojo/verde)
        string _TerrRojo = "0"; public string TerritoriosRojo { get => _TerrRojo; set { _TerrRojo = value; OnPropertyChanged(); } }
        string _TerrVerde = "0"; public string TerritoriosVerde { get => _TerrVerde; set { _TerrVerde = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        // Limpieza
        public void Dispose()
        {
            if (Week?.Live != null && _liveHandler != null)
                Week.Live.PropertyChanged -= _liveHandler;
        }

    }
}
