using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ConcentradoKPI.App.Models
{
    public class IncidentRecord : INotifyPropertyChanged
    {
        private int _no; public int No { get => _no; set { if (_no != value) { _no = value; OnPropertyChanged(); } } }
        private string _uen = "CMA"; public string UEN { get => _uen; set { if (_uen != value) { _uen = value; OnPropertyChanged(); } } }
        private string _nombre = ""; public string NombreInvolucrado { get => _nombre; set { if (_nombre != value) { _nombre = value; OnPropertyChanged(); } } }
        private string _compania = ""; public string CompaniaContratista { get => _compania; set { if (_compania != value) { _compania = value; OnPropertyChanged(); } } }
        private DateTime _fecha = DateTime.Today; public DateTime Fecha { get => _fecha; set { if (_fecha != value) { _fecha = value; OnPropertyChanged(); } } }

        private string _clasificacion = "Incidente";
        public string Clasificacion { get => _clasificacion; set { if (_clasificacion != value) { _clasificacion = value; OnPropertyChanged(); } } }

        private string _responsable = "";
        public string ResponsableSeguridad { get => _responsable; set { if (_responsable != value) { _responsable = value; OnPropertyChanged(); } } }

        private string _observaciones = "";
        public string Observaciones { get => _observaciones; set { if (_observaciones != value) { _observaciones = value; OnPropertyChanged(); } } }

        private string _acciones = "";
        public string Acciones { get => _acciones; set { if (_acciones != value) { _acciones = value; OnPropertyChanged(); } } }

        public IncidentRecord Clone() => (IncidentRecord)MemberwiseClone();

        public void CopyFrom(IncidentRecord other)
        {
            UEN = other.UEN;
            NombreInvolucrado = other.NombreInvolucrado;
            CompaniaContratista = other.CompaniaContratista;
            Fecha = other.Fecha;
            Clasificacion = other.Clasificacion;
            ResponsableSeguridad = other.ResponsableSeguridad;
            Observaciones = other.Observaciones;
            Acciones = other.Acciones;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
