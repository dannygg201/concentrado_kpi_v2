using System;
using System.Collections.Generic;

namespace ConcentradoKPI.App.Models
{
    public class IncidentesDocument
    {
        // Clave del documento
        public string Company { get; set; } = "";
        public string Project { get; set; } = "";
        public int WeekNumber { get; set; }

        // La lista que se muestra en la vista
        public List<IncidentRecord> Items { get; set; } = new();

        // Metadatos
        public int SchemaVersion { get; set; } = 1;
        public DateTime SavedUtc { get; set; } = DateTime.UtcNow;
    }
}
