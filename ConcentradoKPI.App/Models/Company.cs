using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization; // 👈

namespace ConcentradoKPI.App.Models
{
    public class Company : INotifyPropertyChanged
    {
        private string _name = "";
        public string Name
        {
            get => _name;
            set { if (_name != value) { _name = value; OnPropertyChanged(); } }
        }

        public string? Description { get; set; }

        public string? ResponsibleName { get; set; }           // Responsable de la obra/servicio
        public string? ImssRegistration { get; set; }          // Registro patronal IMSS
        public DateTime? HeaderDate { get; set; }              // Fecha del reporte/relación
        public string? Rfc { get; set; }                       // RFC de la compañía
        public string? ProviderNumber { get; set; }            // Número de proveedor
        public string? LegalAddress { get; set; }              // Dirección legal
        public string? SupplierNumber { get; set; }

        // 👇 IMPORTANTE: ahora con set;
        public ObservableCollection<Project> Projects { get; set; } = new();

        // 👇 No lo serialices (estado de UI)
        [JsonIgnore]
        private bool _isExpanded;
        [JsonIgnore]
        public bool IsExpanded
        {
            get => _isExpanded;
            set { if (_isExpanded != value) { _isExpanded = value; OnPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}
