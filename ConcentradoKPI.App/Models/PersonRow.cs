using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;

namespace ConcentradoKPI.App.Models
{
    public class PersonRow : INotifyPropertyChanged
    {
        private int _numero;
        private string _nombre = "";
        private string? _afiliacion;
        private string? _puesto;

        // Asistencia (D,L,M,MM,J,V,S)
        private int _d, _l, _m, _mm, _j, _v, _s;

        public int Numero
        {
            get => _numero;
            set { if (_numero != value) { _numero = value; OnPropertyChanged(); } }
        }

        public string Nombre
        {
            get => _nombre;
            set { if (_nombre != value) { _nombre = value; OnPropertyChanged(); } }
        }

        public string? Afiliacion
        {
            get => _afiliacion;
            set { if (_afiliacion != value) { _afiliacion = value; OnPropertyChanged(); } }
        }

        public string? Puesto
        {
            get => _puesto;
            set { if (_puesto != value) { _puesto = value; OnPropertyChanged(); } }
        }

        public int D { get => _d; set { if (_d != value) { _d = Clamp(value); OnPropertyChanged(); Recalc(); } } }
        public int L { get => _l; set { if (_l != value) { _l = Clamp(value); OnPropertyChanged(); Recalc(); } } }
        public int M { get => _m; set { if (_m != value) { _m = Clamp(value); OnPropertyChanged(); Recalc(); } } }
        public int MM { get => _mm; set { if (_mm != value) { _mm = Clamp(value); OnPropertyChanged(); Recalc(); } } }
        public int J { get => _j; set { if (_j != value) { _j = Clamp(value); OnPropertyChanged(); Recalc(); } } }
        public int V { get => _v; set { if (_v != value) { _v = Clamp(value); OnPropertyChanged(); Recalc(); } } }
        public int S { get => _s; set { if (_s != value) { _s = Clamp(value); OnPropertyChanged(); Recalc(); } } }

        // HH de la semana = suma de L,M,MM,J,V,S (D normalmente es 0)
        private int _hhSemana;
        public int HHSemana
        {
            get => _hhSemana;
            private set { if (_hhSemana != value) { _hhSemana = value; OnPropertyChanged(); } }
        }

        private static int Clamp(int v) => Math.Max(0, Math.Min(24, v));

        private void Recalc()
        {
            HHSemana = D + L + M + MM + J + V + S;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
