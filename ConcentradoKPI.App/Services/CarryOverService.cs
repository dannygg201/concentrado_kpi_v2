// Services/CarryOverService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using ConcentradoKPI.App.Models;

namespace ConcentradoKPI.App.Services
{
    public static class CarryOverService
    {
        /// <summary>
        /// Copia estructura desde la semana previa del mismo proyecto:
        /// - Clona tablas y pone horas/números en 0 (heurística).
        /// - Clona Personal Vigente con horas = 0 y recalcula Live.
        /// - Clona la pirámide y fuerza Colaboradores/Técnicos/Horas desde Live (no arrastra de la semana previa).
        /// - Ajusta Week.Notes con traza.
        /// </summary>
        public static void ApplyFromPrevious(Project project, WeekData targetWeek)
        {
            if (project == null || targetWeek == null) return;

            var prev = project.Weeks
                              .Where(w => w.WeekNumber < targetWeek.WeekNumber)
                              .OrderByDescending(w => w.WeekNumber)
                              .FirstOrDefault();
            if (prev == null) return;

            bool clonedSomething = false;

            // 1) TABLAS: clonar estructura reseteando números/horas a 0
            if (prev.Tables != null && prev.Tables.Count > 0)
            {
                targetWeek.Tables = new List<TableData>(prev.Tables.Count);
                foreach (var t in prev.Tables)
                    targetWeek.Tables.Add(CloneTableResetNumbers(t));
                clonedSomething = true;
            }

            // 2) PERSONAL VIGENTE: clonar encabezado + personal con horas = 0
            if (prev.PersonalVigenteDocument != null)
            {
                var src = prev.PersonalVigenteDocument;
                var copy = new PersonalVigenteDocument
                {
                    Company = src.Company,
                    Project = src.Project,
                    WeekNumber = targetWeek.WeekNumber,
                    RazonSocial = src.RazonSocial,
                    ResponsableObra = src.ResponsableObra,
                    RegistroIMSS = src.RegistroIMSS,
                    RFCCompania = src.RFCCompania,
                    DireccionLegal = src.DireccionLegal,
                    NumeroProveedor = src.NumeroProveedor,
                    Fecha = DateTime.Today,
                    Personal = new List<PersonRow>()
                };

                foreach (var p in src.Personal)
                {
                    copy.Personal.Add(new PersonRow
                    {
                        Numero = p.Numero,
                        Nombre = p.Nombre,
                        Afiliacion = p.Afiliacion,
                        Puesto = p.Puesto,
                        EsTecnicoSeguridad = p.EsTecnicoSeguridad,
                        D = 0,
                        L = 0,
                        M = 0,
                        MM = 0,
                        J = 0,
                        V = 0,
                        S = 0
                    });
                }

                targetWeek.PersonalVigenteDocument = copy;
                clonedSomething = true;

                // ✅ recalcular Live a partir del PV con horas en 0
                LiveSyncService.Recalc(targetWeek, copy);
            }
            else
            {
                // Sin PV previo: deja Live en 0 y notifica
                LiveSyncService.Recalc(targetWeek, null);
            }

            // 3) PIRÁMIDE: clonar y FORZAR 3 campos desde Live (no arrastrar horas/personas/técnicos)
            if (prev.PiramideSeguridad != null)
            {
                targetWeek.PiramideSeguridad = ClonePiramideDocument(prev.PiramideSeguridad, targetWeek.WeekNumber);

                // forzar los 3 totales desde Live (que ya está recalculado)
                targetWeek.PiramideSeguridad.Colaboradores = targetWeek.Live.ColaboradoresTotal;
                targetWeek.PiramideSeguridad.TecnicosSeguridad = targetWeek.Live.TecnicosSeguridadTotal;
                targetWeek.PiramideSeguridad.HorasTrabajadas = targetWeek.Live.HorasTrabajadasTotal;

                // (opcional) hidratar estructura en vivo
                targetWeek.Pyramid = SafetyFromDoc(targetWeek.PiramideSeguridad);

                clonedSomething = true;
            }
            else if (prev.Pyramid != null)
            {
                // Soporte heredado: clonar la estructura "en vivo"
                targetWeek.Pyramid = new SafetyPyramid
                {
                    UnsafeActs = prev.Pyramid.UnsafeActs,
                    UnsafeConditions = prev.Pyramid.UnsafeConditions,
                    NearMisses = prev.Pyramid.NearMisses,
                    FirstAids = prev.Pyramid.FirstAids,
                    MedicalTreatments = prev.Pyramid.MedicalTreatments,
                    LostTimeInjuries = prev.Pyramid.LostTimeInjuries,
                    Fatalities = prev.Pyramid.Fatalities,
                    FindingsCorrected = prev.Pyramid.FindingsCorrected,
                    FindingsTotal = prev.Pyramid.FindingsTotal,
                    TrainingsCompleted = prev.Pyramid.TrainingsCompleted,
                    TrainingsPlanned = prev.Pyramid.TrainingsPlanned,
                    AuditsCompleted = prev.Pyramid.AuditsCompleted,
                    AuditsPlanned = prev.Pyramid.AuditsPlanned
                };
                clonedSomething = true;
            }

            // 4) Nota de trazabilidad
            if (string.IsNullOrWhiteSpace(targetWeek.Notes))
                targetWeek.Notes = clonedSomething
                    ? $"Semana {targetWeek.WeekNumber} creada a partir de semana {prev.WeekNumber}"
                    : $"Semana {targetWeek.WeekNumber} creada (sin datos previos)";
        }

        // ==================== Helpers ====================

        private static TableData CloneTableResetNumbers(TableData src)
        {
            var clone = new TableData
            {
                Name = src.Name,
                Columns = src.Columns?.ToList() ?? new List<string>(),
                Personas = new List<List<string>>()
            };

            if (src.Personas != null)
            {
                foreach (var row in src.Personas)
                {
                    var newRow = new List<string>(row.Count);
                    for (int i = 0; i < row.Count; i++)
                    {
                        var colName = i < (src.Columns?.Count ?? 0) ? src.Columns[i] : string.Empty;
                        var val = row[i];

                        // Heurística: si la columna parece de horas/número → 0
                        if (LooksLikeHours(colName) || IsNumeric(val))
                            newRow.Add("0");
                        else
                            newRow.Add(val);
                    }
                    clone.Personas.Add(newRow);
                }
            }

            return clone;
        }

        private static bool LooksLikeHours(string? colName)
        {
            if (string.IsNullOrWhiteSpace(colName)) return false;
            colName = colName.Trim().ToLowerInvariant();
            return colName.Contains("hora") || colName == "hrs" || colName == "h" || colName.Contains("tiempo");
        }

        private static bool IsNumeric(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            return double.TryParse(
                s.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out _);
        }

        private static PiramideSeguridadDocument ClonePiramideDocument(PiramideSeguridadDocument src, int newWeekNumber)
        {
            return new PiramideSeguridadDocument
            {
                Company = src.Company,
                Project = src.Project,
                WeekNumber = newWeekNumber,

                // Laterales / generales
                Companias = src.Companias,
                Colaboradores = src.Colaboradores,
                TecnicosSeguridad = src.TecnicosSeguridad,
                HorasTrabajadas = src.HorasTrabajadas,
                WithoutLTIs = src.WithoutLTIs,
                LastRecord = src.LastRecord,

                // Base
                Seguros = src.Seguros,
                Inseguros = src.Inseguros,
                Detectadas = src.Detectadas,
                Corregidas = src.Corregidas,
                Avance = src.Avance,
                AvanceProgramaPct = src.AvanceProgramaPct,
                Efectividad = src.Efectividad,
                TerritoriosRojo = src.TerritoriosRojo,
                TerritoriosVerde = src.TerritoriosVerde,

                // Centro
                Potenciales = src.Potenciales,
                Precursores1 = src.Precursores1,
                Precursores2 = src.Precursores2,
                Precursores3 = src.Precursores3,

                // Incidentes sin lesión
                IncidentesSinLesion1 = src.IncidentesSinLesion1,
                IncidentesSinLesion2 = src.IncidentesSinLesion2,

                // Niveles
                FAI1 = src.FAI1,
                FAI2 = src.FAI2,
                FAI3 = src.FAI3,
                MTI1 = src.MTI1,
                MTI2 = src.MTI2,
                MTI3 = src.MTI3,
                MDI1 = src.MDI1,
                MDI2 = src.MDI2,
                MDI3 = src.MDI3,
                LTI1 = src.LTI1,
                LTI2 = src.LTI2,
                LTI3 = src.LTI3,

                SchemaVersion = src.SchemaVersion,
                SavedUtc = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Convierte el documento persistido a la estructura "en vivo" SafetyPyramid.
        /// </summary>
        private static SafetyPyramid SafetyFromDoc(PiramideSeguridadDocument d)
        {
            int Sum(params int[] xs) => xs?.Sum() ?? 0;

            return new SafetyPyramid
            {
                UnsafeActs = d.Inseguros,
                UnsafeConditions = 0,
                NearMisses = d.Potenciales,
                FirstAids = Sum(d.FAI1, d.FAI2, d.FAI3),
                MedicalTreatments = Sum(d.MTI1, d.MTI2, d.MTI3),
                LostTimeInjuries = Sum(d.LTI1, d.LTI2, d.LTI3),
                Fatalities = 0,
                FindingsCorrected = d.Corregidas,
                FindingsTotal = d.Detectadas,
                TrainingsCompleted = 0,
                TrainingsPlanned = 0,
                AuditsCompleted = 0,
                AuditsPlanned = 0
            };
        }
    }
}
