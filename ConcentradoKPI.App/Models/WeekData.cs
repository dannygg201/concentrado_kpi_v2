using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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

        public override string ToString() => $"Semana {WeekNumber}";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
        // Guarda todo lo capturado en la vista Personal Vigente para esta semana
        public PersonalVigenteDocument? PersonalVigente { get; set; }

    }
}
