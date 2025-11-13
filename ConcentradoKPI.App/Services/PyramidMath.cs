using System;
using ConcentradoKPI.App.Models;

namespace ConcentradoKPI.App.Services
{
    /// <summary>
    /// Reglas de mapeo entre Pirámide (base) y el Informe Semanal CMA (ISCMA).
    /// - Effective = Base + ISCMA
    /// - Base = Effective - ISCMA
    /// Nota: NO sumamos Colaboradores / Técnicos / Horas (vienen de Live).
    /// </summary>
    public static class PyramidMath
    {
        public static PiramideValues AddWeekly(PiramideValues baseV, InformeSemanalCmaDocument? w)
        {
            if (w == null) return baseV.Clone();

            var r = baseV.Clone();

            // Actos
            r.Seguros = SafeAdd(r.Seguros, w.ActosSeguros);
            r.Inseguros = SafeAdd(r.Inseguros, w.ActosInseguros);

            // Precursores: comportamiento -> Precursores1, condición -> Detectadas
            r.Precursores1 = SafeAdd(r.Precursores1, w.PrecursoresSifComportamiento);
            r.Detectadas = SafeAdd(r.Detectadas, w.PrecursoresSifCondicion);

            // Incidentes sin lesión: lo acumulamos en el primer cuadro
            r.IncidentesSinLesion1 = SafeAdd(r.IncidentesSinLesion1, w.Incidentes);

            // FAI / MTI / MDI / LTI totales -> bloque 1 (si necesitas otra distribución, luego lo afinamos)
            r.FAI1 = SafeAdd(r.FAI1, w.FAI);
            r.MTI1 = SafeAdd(r.MTI1, w.MTI);
            r.MDI1 = SafeAdd(r.MDI1, w.MDI);
            r.LTI1 = SafeAdd(r.LTI1, w.LTI);

            // Avance / efectividad / territorios NO se tocan aquí (son propios del base o calculados)
            // Potenciales tampoco (ISCMA no los reporta)
            // AvanceProgramaPct tampoco (ISCMA no lo maneja como pct acumulable)

            return r;
        }

        public static PiramideValues SubtractWeekly(PiramideValues effective, InformeSemanalCmaDocument? w)
        {
            if (w == null) return effective.Clone();

            var r = effective.Clone();

            // Inversa de lo anterior (clamp >= 0)
            r.Seguros = SafeSub(r.Seguros, w.ActosSeguros);
            r.Inseguros = SafeSub(r.Inseguros, w.ActosInseguros);

            r.Precursores1 = SafeSub(r.Precursores1, w.PrecursoresSifComportamiento);
            r.Detectadas = SafeSub(r.Detectadas, w.PrecursoresSifCondicion);

            r.IncidentesSinLesion1 = SafeSub(r.IncidentesSinLesion1, w.Incidentes);

            r.FAI1 = SafeSub(r.FAI1, w.FAI);
            r.MTI1 = SafeSub(r.MTI1, w.MTI);
            r.MDI1 = SafeSub(r.MDI1, w.MDI);
            r.LTI1 = SafeSub(r.LTI1, w.LTI);

            return r;
        }

        private static int SafeAdd(int a, int b) => a + b;
        private static int SafeSub(int a, int b) => Math.Max(0, a - b);
    }
}
