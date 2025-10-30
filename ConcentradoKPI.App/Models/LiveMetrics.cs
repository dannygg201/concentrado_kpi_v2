using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ConcentradoKPI.App.Models
{
    public class LiveMetrics : INotifyPropertyChanged
    {
        private int _colaboradoresTotal;
        public int ColaboradoresTotal
        {
            get => _colaboradoresTotal;
            set { if (_colaboradoresTotal != value) { _colaboradoresTotal = value; OnPropertyChanged(); } }
        }

        private int _horasTrabajadasTotal;
        public int HorasTrabajadasTotal
        {
            get => _horasTrabajadasTotal;
            set { if (_horasTrabajadasTotal != value) { _horasTrabajadasTotal = value; OnPropertyChanged(); } }
        }


        private int _tecnicosSeguridadTotal;
        public int TecnicosSeguridadTotal
        {
            get => _tecnicosSeguridadTotal;
            set { if (_tecnicosSeguridadTotal != value) { _tecnicosSeguridadTotal = value; OnPropertyChanged(); } }
        }

        public void NotifyAll() => OnPropertyChanged(string.Empty);

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
