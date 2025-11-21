using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ConcentradoKPI.App.Models
{
    public class PrecursorSifRecord : INotifyPropertyChanged
    {
        private int _no;
        public int No
        {
            get => _no;
            set { if (_no != value) { _no = value; OnPropertyChanged(); } }
        }

        private string _uen = "CMC";
        public string UEN
        {
            get => _uen;
            set { if (_uen != value) { _uen = value; OnPropertyChanged(); } }
        }

        private string _nombreInvolucrado = "";
        public string NombreInvolucrado
        {
            get => _nombreInvolucrado;
            set { if (_nombreInvolucrado != value) { _nombreInvolucrado = value; OnPropertyChanged(); } }
        }

        private string _companiaContratista = "";
        public string CompaniaContratista
        {
            get => _companiaContratista;
            set { if (_companiaContratista != value) { _companiaContratista = value; OnPropertyChanged(); } }
        }

        // Si prefieres Nullable<DateTime?> cambia el tipo y listo;
        // el XAML funciona bien con DateTime también.
        private DateTime _fecha = DateTime.Today;
        public DateTime Fecha
        {
            get => _fecha;
            set { if (_fecha != value) { _fecha = value; OnPropertyChanged(); } }
        }

        // NUEVO: campo "Precursor SIF" (textbox)
        private string _precursorSif = "";
        public string PrecursorSif
        {
            get => _precursorSif;
            set { if (_precursorSif != value) { _precursorSif = value; OnPropertyChanged(); } }
        }

        // "Tipo de precursor" ahora es TextBox (no catálogo)
        private string _tipoPrecursor = "";
        public string TipoPrecursor
        {
            get => _tipoPrecursor;
            set { if (_tipoPrecursor != value) { _tipoPrecursor = value; OnPropertyChanged(); } }
        }

        private string _descripcion = "";
        public string Descripcion
        {
            get => _descripcion;
            set { if (_descripcion != value) { _descripcion = value; OnPropertyChanged(); } }
        }

        // NUEVO: "Acciones" (textbox)
        private string _acciones = "";
        public string Acciones
        {
            get => _acciones;
            set { if (_acciones != value) { _acciones = value; OnPropertyChanged(); } }
        }

        // (Opcionales – los dejo por si los usas en otra hoja;
        // si no, puedes borrarlos sin problema)
        private string _clasificacion = "";
        public string Clasificacion
        {
            get => _clasificacion;
            set { if (_clasificacion != value) { _clasificacion = value; OnPropertyChanged(); } }
        }

        private string _responsableSeguridad = "";
        public string ResponsableSeguridad
        {
            get => _responsableSeguridad;
            set { if (_responsableSeguridad != value) { _responsableSeguridad = value; OnPropertyChanged(); } }
        }

        private string _observaciones = "";
        public string Observaciones
        {
            get => _observaciones;
            set { if (_observaciones != value) { _observaciones = value; OnPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));

        public PrecursorSifRecord Clone() => (PrecursorSifRecord)MemberwiseClone();

        public void CopyFrom(PrecursorSifRecord other)
        {
            UEN = other.UEN;
            NombreInvolucrado = other.NombreInvolucrado;
            CompaniaContratista = other.CompaniaContratista;
            Fecha = other.Fecha;
            PrecursorSif = other.PrecursorSif;        // <-- nuevo
            TipoPrecursor = other.TipoPrecursor;
            Descripcion = other.Descripcion;
            Acciones = other.Acciones;                // <-- nuevo
            Clasificacion = other.Clasificacion;
            ResponsableSeguridad = other.ResponsableSeguridad;
            Observaciones = other.Observaciones;
        }
    }
}
