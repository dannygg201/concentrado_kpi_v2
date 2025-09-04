using System;
using System.Collections.ObjectModel;

namespace ConcentradoKPI.App.Models
{
    // Raíz que se guarda/carga en JSON
    public class AppData
    {
        public string SchemaVersion { get; set; } = "1.0.0";
        public string AppName { get; set; } = "ConcentradoKPI";
        public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
        public ObservableCollection<Company> Companies { get; set; } = new();
    }
}
