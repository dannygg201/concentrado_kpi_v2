using System;
using System.Collections.Generic;

namespace ConcentradoKPI.App.Models
{
    public class PrecursorSifDocument
    {
        // Clave del documento (para WeekData)
        public string Company { get; set; } = "";
        public string Project { get; set; } = "";
        public int WeekNumber { get; set; }

        // La lista que ves en la tabla
        public List<PrecursorSifRecord> Items { get; set; } = new();

        // Metadatos
        public int SchemaVersion { get; set; } = 1;
        public DateTime SavedUtc { get; set; } = DateTime.UtcNow;
    }
}
