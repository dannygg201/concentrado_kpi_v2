namespace ConcentradoKPI.App.Models
{
    public class PiramideValues
    {
        // Tarjetas laterales e indicadores generales
        public int Companias { get; set; }
        public int Colaboradores { get; set; }
        public int TecnicosSeguridad { get; set; }
        public int HorasTrabajadas { get; set; }
        public int WithoutLTIs { get; set; }
        public string? LastRecord { get; set; } // si lo manejas como string en pantalla

        public int TerritoriosRojo { get; set; }
        public int TerritoriosVerde { get; set; }


        // Base y condiciones
        public int Seguros { get; set; }
        public int Inseguros { get; set; }
        public int Detectadas { get; set; }
        public int Corregidas { get; set; }
        public int Avance { get; set; }               // si ese “Avance” es numérico
        public int AvanceProgramaPct { get; set; }    // el de “Avance al Programa %”
        public int Efectividad { get; set; }

        // Precursores / Potenciales (centro)
        public int Potenciales { get; set; }
        public int Precursores1 { get; set; }
        public int Precursores2 { get; set; }
        public int Precursores3 { get; set; }

        // Incidentes sin lesión
        public int IncidentesSinLesion1 { get; set; }
        public int IncidentesSinLesion2 { get; set; }

        // FAI / MTI / MDI / LTI (los bloques)
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

        public PiramideValues Clone() => (PiramideValues)MemberwiseClone();
    }
}
