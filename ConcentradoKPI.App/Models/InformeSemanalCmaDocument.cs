using System;

namespace ConcentradoKPI.App.Models
{
    public class InformeSemanalCmaDocument
    {
        // Clave del documento
        public string Company { get; set; } = "";
        public string Project { get; set; } = "";
        public int WeekNumber { get; set; }

        // Fila editable
        public string Nombre { get; set; } = "";
        public string Especialidad { get; set; } = "";

        public int TecnicosSeguridad { get; set; }
        public int Colaboradores { get; set; }
        public int HorasTrabajadas { get; set; }

        public int LTI { get; set; }
        public int MDI { get; set; }
        public int MTI { get; set; }
        public int TRI { get; set; }
        public int FAI { get; set; }

        public int Incidentes { get; set; }

        public int PrecursoresSifComportamiento { get; set; }
        public int PrecursoresSifCondicion { get; set; }

        public int ActosSeguros { get; set; }
        public int ActosInseguros { get; set; }

        // 🔹 NUEVOS: Corregidas / Detectadas (condiciones)
        public int Corregidas { get; set; }
        public int Detectadas { get; set; }

        // Derivados
        public int TotalSemanal { get; set; }
        public double PorcentajeAvance { get; set; }

        // Metadatos
        public int SchemaVersion { get; set; } = 2;
        public DateTime SavedUtc { get; set; } = DateTime.UtcNow;
    }
}
