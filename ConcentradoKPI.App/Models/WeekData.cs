using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace ConcentradoKPI.App.Models
{
    public class WeekData : INotifyPropertyChanged
    {
        private int _weekNumber;
        public int WeekNumber
        {
            get => _weekNumber;
            set { if (_weekNumber != value) { _weekNumber = value; OnPropertyChanged(); } }
        }

        public DateTime? WeekStart { get; set; }
        public DateTime? WeekEnd { get; set; }
        public List<KpiValue>? Kpis { get; set; }
        public SafetyPyramid? Pyramid { get; set; }
        public List<TableData>? Tables { get; set; }
        public string? Notes { get; set; }

        // Documentos persistidos (pueden estar desincronizados del “en vivo”)
        public PersonalVigenteDocument? PersonalVigente { get; set; }
        public PersonalVigenteDocument? PersonalVigenteDocument
        {
            get => PersonalVigente;
            set => PersonalVigente = value;
        }
        private PiramideSeguridadDocument? _piramideSeguridad;
        public PiramideSeguridadDocument? PiramideSeguridad
        {
            get => _piramideSeguridad;
            set
            {
                if (!Equals(_piramideSeguridad, value))
                {
                    _piramideSeguridad = value;
                    OnPropertyChanged();
                }
            }
        }

        private InformeSemanalCmaDocument? _informeSemanalCma;
        public InformeSemanalCmaDocument? InformeSemanalCma
        {
            get => _informeSemanalCma;
            set
            {
                if (!Equals(_informeSemanalCma, value))
                {
                    _informeSemanalCma = value;
                    OnPropertyChanged();
                }
            }
        }

        public PrecursorSifDocument? PrecursorSif { get; set; }
        public IncidentesDocument? Incidentes { get; set; }

        // 🔴 ÚNICA fuente de verdad en vivo (inmutable en referencia)
        public LiveMetrics Live { get; } = new LiveMetrics();

        public override string ToString() => $"Semana {WeekNumber}";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}
