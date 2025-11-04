//// Services/WeekCarryOverService.cs
//using System.Threading.Tasks;
//using ConcentradoKPI.App.Models;

//namespace ConcentradoKPI.App.Services
//{
//    public interface IWeekCarryOverService
//    {
//        Task ApplyFromMostRecentAsync(Company c, Project p, WeekData targetWeek);
//    }

//    public sealed class WeekCarryOverService : IWeekCarryOverService
//    {
//        private readonly IWeekRepository _repo;
//        public WeekCarryOverService(IWeekRepository repo) => _repo = repo;

//        public async Task ApplyFromMostRecentAsync(Company c, Project p, WeekData targetWeek)
//        {
//            // 1) Buscar la semana inmediatamente anterior (más reciente)
//            var prev = await _repo.GetLastWeekAsync(
//                companyId: c.Id,
//                projectId: p.Id,
//                beforeYear: targetWeek.IsoYear,
//                beforeWeek: targetWeek.IsoWeek
//            );
//            if (prev is null) return;

//            // 2) Empresa: arrastrar tal cual
//            if (prev.PersonalVigente is not null)
//            {
//                targetWeek.PersonalVigente ??= new PersonalVigenteData();
//                targetWeek.PersonalVigente.EmpresaNombre = prev.PersonalVigente.EmpresaNombre;
//                targetWeek.PersonalVigente.RazonSocial = prev.PersonalVigente.RazonSocial;
//                targetWeek.PersonalVigente.RFC = prev.PersonalVigente.RFC;
//                targetWeek.PersonalVigente.DomicilioFiscal = prev.PersonalVigente.DomicilioFiscal;
//                targetWeek.PersonalVigente.Representante = prev.PersonalVigente.Representante;
//                targetWeek.PersonalVigente.TelefonoContacto = prev.PersonalVigente.TelefonoContacto;
//                targetWeek.PersonalVigente.CorreoContacto = prev.PersonalVigente.CorreoContacto;

//                // 3) Personal: copiar lista de la semana previa (ya incluye altas/bajas más recientes) con horas = 0
//                targetWeek.PersonalVigente.Personal = prev.PersonalVigente.Personal?
//                    .Select(p => new PersonaVigente
//                    {
//                        Id = p.Id, // si tu Id es estable por persona
//                        NombreCompleto = p.NombreCompleto,
//                        Puesto = p.Puesto,
//                        Area = p.Area,
//                        NoEmpleado = p.NoEmpleado,
//                        TipoContrato = p.TipoContrato,
//                        Turno = p.Turno,
//                        // …los campos que ya manejas…
//                        HorasTrabajadas = 0 // clave: horas en cero
//                    })
//                    .ToList();

//                // Total de horas semanal en 0
//                targetWeek.PersonalVigente.HorasTrabajadasTotal = 0;
//            }

//            // 4) Pirámide: clonar estructura/datos, dejando fuera horas (en 0 o null)
//            if (prev.PiramideSeguridad is not null)
//            {
//                targetWeek.PiramideSeguridad = new PiramideSeguridadData
//                {
//                    // Copia aquí toda la estructura que “no es horas” (niveles, totales por tipo, etc.)
//                    // Ejemplos ilustrativos (ajusta a tus nombres reales):
//                    FAI = prev.PiramideSeguridad.FAI,
//                    MTI = prev.PiramideSeguridad.MTI,
//                    MDI = prev.PiramideSeguridad.MDI,
//                    LTI = prev.PiramideSeguridad.LTI,
//                    Precursores = prev.PiramideSeguridad.Precursores?.Select(x => x.Clone()).ToList(),
//                    Potenciales = prev.PiramideSeguridad.Potenciales?.Select(x => x.Clone()).ToList(),

//                    // Horas relacionadas con la pirámide en 0 (si las manejas ahí):
//                    HorasTrabajadasTotal = 0
//                };

//                // Si llevas horas por subitem en la pirámide, ponlas en 0 aquí también.
//            }

//            // 5) Bandera opcional para tu UI
//            targetWeek.Flags ??= new WeekFlags();
//            targetWeek.Flags.PendienteCapturarHorasTrabajadas = true;
//        }
//    }
//}
