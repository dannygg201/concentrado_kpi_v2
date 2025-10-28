// Models/PiramideSeguridadDocument.cs
using System;

namespace ConcentradoKPI.App.Models
{
    public class PiramideSeguridadDocument
    {
        public string Company { get; set; } = "";
        public string Project { get; set; } = "";
        public int WeekNumber { get; set; }

        // Laterales / generales
        public int Companias { get; set; }
        public int Colaboradores { get; set; }
        public int TecnicosSeguridad { get; set; }
        public int HorasTrabajadas { get; set; }
        public int WithoutLTIs { get; set; }
        public string? LastRecord { get; set; } // yyyy-MM-dd o null

        // Base
        public int Seguros { get; set; }
        public int Inseguros { get; set; }
        public int Detectadas { get; set; }
        public int Corregidas { get; set; }
        public int Avance { get; set; }
        public int AvanceProgramaPct { get; set; }
        public int Efectividad { get; set; }
        public int TerritoriosRojo { get; set; }
        public int TerritoriosVerde { get; set; }

        // Centro
        public int Potenciales { get; set; }
        public int Precursores1 { get; set; }
        public int Precursores2 { get; set; }
        public int Precursores3 { get; set; }

        // Incidentes sin lesión
        public int IncidentesSinLesion1 { get; set; }
        public int IncidentesSinLesion2 { get; set; }

        // Niveles
        public int FAI1 { get; set; }
        public int FAI2 { get; set; }
        public int FAI3 { get; set; }
        public int MTI1 { get; set; }
        public int MTI2 { get; set; }
        public int MTI3 { get; set; }
        public int MDI1 { get; set; }
        public int MDI2 { get; set; }
        public int MDI3 { get; set; }
        public int LTI1 { get; set; }
        public int LTI2 { get; set; }
        public int LTI3 { get; set; }

        // Metadatos
        public int SchemaVersion { get; set; } = 1;
        public DateTime SavedUtc { get; set; } = DateTime.UtcNow;
    }
}
