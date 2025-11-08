using System.Linq;
using ConcentradoKPI.App.Models;

namespace ConcentradoKPI.App.Services
{
    public static class LiveSyncService
    {
        public static void Recalc(WeekData week, PersonalVigenteDocument? doc = null)
        {
            if (week == null) return;
            doc ??= week.PersonalVigenteDocument;

            if (doc == null)
            {
                week.Live.ColaboradoresTotal = 0;
                week.Live.TecnicosSeguridadTotal = 0;
                week.Live.HorasTrabajadasTotal = 0;
                return;
            }

            int colaboradores = doc.Personal?.Count ?? 0;
            int tecnicos = doc.Personal?.Count(p => p.EsTecnicoSeguridad) ?? 0;
            int SumaHoras(PersonRow p) => p.D + p.L + p.M + p.MM + p.J + p.V + p.S;
            int horas = doc.Personal?.Sum(SumaHoras) ?? 0;

            week.Live.ColaboradoresTotal = colaboradores;
            week.Live.TecnicosSeguridadTotal = tecnicos;
            week.Live.HorasTrabajadasTotal = horas;

        }
    }
}
