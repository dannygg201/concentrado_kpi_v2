using System;
using System.Collections.Generic;

namespace ConcentradoKPI.App.Models
{
    public class PersonalVigenteDocument
    {
        // Clave del documento
        public string Company { get; set; } = "";
        public string Project { get; set; } = "";
        public int WeekNumber { get; set; }

        // Encabezado
        public string RazonSocial { get; set; } = "";
        public string ResponsableObra { get; set; } = "";
        public string RegistroIMSS { get; set; } = "";
        public string RFCCompania { get; set; } = "";
        public string DireccionLegal { get; set; } = "";
        public string NumeroProveedor { get; set; } = "";
        public DateTime? Fecha { get; set; }

        // Detalle (tu relación de personal con horas)
        public List<PersonRow> Personal { get; set; } = new();

        // Metadatos
        public int SchemaVersion { get; set; } = 1;
        public DateTime SavedUtc { get; set; } = DateTime.UtcNow;
    }
}
