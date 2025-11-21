using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.Services;
using ConcentradoKPI.App.ViewModels;
using ConcentradoKPI.App.Views.Abstractions;

namespace ConcentradoKPI.App.Views.Pages
{
    public partial class InformeSemanalCmaView : UserControl, ISyncToWeek
    {
        private Company? _company;
        private Project? _project;
        private WeekData? _week;

        public InformeSemanalCmaView()
        {
            InitializeComponent();

            // 🔹 Cuando cambie el VM, me guardo Company/Project/Week
            DataContextChanged += OnDataContextChanged;

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                var wLocal = new WeekData();
                DataContext = new InformeSemanalCMAViewModel(
                    new Company { Name = "Demo Co." },
                    new Project { Name = "Proyecto Demo" },
                    wLocal
                );
            }

            // Por si se descarga la vista (cambiar de pestaña, etc.)
            Unloaded += (_, __) => SyncWeekAndPyramid();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is InformeSemanalCMAViewModel vm)
            {
                _company = vm.Company;
                _project = vm.Project;
                _week = vm.Week;
            }
        }

        // Opcional (si algún día quieres usar InitContext de otra forma)
        public void InitContext(Company c, Project p, WeekData w)
        {
            _company = c;
            _project = p;
            _week = w;

            if (DataContext is not InformeSemanalCMAViewModel)
                DataContext = new InformeSemanalCMAViewModel(c, p, w);
        }

        public void FlushToWeek() => SyncWeekAndPyramid();
        public void SyncIntoWeek() => SyncWeekAndPyramid();

        private void SyncWeekAndPyramid()
        {
            if (_company == null || _project == null || _week == null) return;
            if (DataContext is not InformeSemanalCMAViewModel vm) return;

            // === 1) Documento semanal viejo / nuevo ===
            var oldWeekly = _week.InformeSemanalCma;

            var newWeekly = BuildWeeklyFromVm(
                vm.Editable,
                _company.Name,
                _project.Name,
                _week.WeekNumber
            );

            bool weeklyChanged = !WeeklyEquals(oldWeekly, newWeekly);

            // === 2) Pirámide vieja / nueva ===
            PiramideSeguridadDocument? oldPiramDoc =
                _week.PiramideSeguridad as PiramideSeguridadDocument;

            PiramideValues pir;
            if (oldPiramDoc != null)
                pir = PiramideSeguridadView.FromDocument(oldPiramDoc);
            else
                pir = new PiramideValues();

            // Espejo de lo que diga el informe semanal
            pir.Seguros = newWeekly.ActosSeguros;
            pir.Inseguros = newWeekly.ActosInseguros;
            pir.Precursores1 = newWeekly.PrecursoresSifComportamiento;
            pir.Precursores2 = newWeekly.PrecursoresSifCondicion;
            pir.IncidentesSinLesion1 = newWeekly.Incidentes;
            pir.FAI1 = newWeekly.FAI;
            pir.MTI1 = newWeekly.MTI;
            pir.MDI1 = newWeekly.MDI;
            pir.LTI1 = newWeekly.LTI;
            pir.Detectadas = newWeekly.Detectadas;
            pir.Corregidas = newWeekly.Corregidas;

            var newPiramDoc = PiramideSeguridadView.ToDocument(
                pir,
                _company.Name,
                _project.Name,
                _week.WeekNumber
            );

            bool pirChanged = !PiramideEquals(oldPiramDoc, newPiramDoc);

            // === 3) Si NO cambió nada, no ensuciamos el proyecto ===
            if (!weeklyChanged && !pirChanged)
                return;

            // === 4) Aplicar cambios reales y marcar dirty ===
            _week.InformeSemanalCma = newWeekly;
            _week.PiramideSeguridad = newPiramDoc;

            ProjectStorageService.MarkDirty();
        }

        private static InformeSemanalCmaDocument BuildWeeklyFromVm(
            ContratistaRow row,
            string company,
            string project,
            int weekNumber)
        {
            return new InformeSemanalCmaDocument
            {
                Company = company,
                Project = project,
                WeekNumber = weekNumber,

                Nombre = row.Nombre,
                Especialidad = row.Especialidad,
                Colaboradores = row.Colaboradores,
                TecnicosSeguridad = row.TecnicosSeguridad,
                HorasTrabajadas = row.HorasTrabajadas,

                LTI = row.LTI,
                MDI = row.MDI,
                MTI = row.MTI,
                TRI = row.TRI,
                FAI = row.FAI,

                Incidentes = row.Incidentes,
                PrecursoresSifComportamiento = row.PrecursoresSifComportamiento,
                PrecursoresSifCondicion = row.PrecursoresSifCondicion,
                ActosSeguros = row.ActosSeguros,
                ActosInseguros = row.ActosInseguros,

                Corregidas = row.Corregidas,
                Detectadas = row.Detectadas,

                TotalSemanal = row.TotalSemanal,
                PorcentajeAvance = row.PorcentajeAvance,

                SchemaVersion = 2,
                SavedUtc = DateTime.UtcNow
            };
        }

        // ===== Helpers de comparación =====

        private static bool WeeklyEquals(InformeSemanalCmaDocument? a, InformeSemanalCmaDocument b)
        {
            if (a == null) return false;

            return
                a.Company == b.Company &&
                a.Project == b.Project &&
                a.WeekNumber == b.WeekNumber &&

                a.Nombre == b.Nombre &&
                a.Especialidad == b.Especialidad &&

                a.TecnicosSeguridad == b.TecnicosSeguridad &&
                a.Colaboradores == b.Colaboradores &&
                a.HorasTrabajadas == b.HorasTrabajadas &&

                a.LTI == b.LTI &&
                a.MDI == b.MDI &&
                a.MTI == b.MTI &&
                a.TRI == b.TRI &&
                a.FAI == b.FAI &&

                a.Incidentes == b.Incidentes &&
                a.PrecursoresSifComportamiento == b.PrecursoresSifComportamiento &&
                a.PrecursoresSifCondicion == b.PrecursoresSifCondicion &&

                a.ActosSeguros == b.ActosSeguros &&
                a.ActosInseguros == b.ActosInseguros &&

                a.Corregidas == b.Corregidas &&
                a.Detectadas == b.Detectadas;
            // Ignoramos TotalSemanal, PorcentajeAvance, SavedUtc, SchemaVersion
        }

        private static bool PiramideEquals(PiramideSeguridadDocument? a, PiramideSeguridadDocument b)
        {
            if (a == null) return false;

            return
                a.Company == b.Company &&
                a.Project == b.Project &&
                a.WeekNumber == b.WeekNumber &&

                a.Seguros == b.Seguros &&
                a.Inseguros == b.Inseguros &&
                a.Precursores1 == b.Precursores1 &&
                a.Precursores2 == b.Precursores2 &&
                a.IncidentesSinLesion1 == b.IncidentesSinLesion1 &&
                a.FAI1 == b.FAI1 &&
                a.MTI1 == b.MTI1 &&
                a.MDI1 == b.MDI1 &&
                a.LTI1 == b.LTI1 &&
                a.Detectadas == b.Detectadas &&
                a.Corregidas == b.Corregidas;
            // El resto de campos de la pirámide se dejan fuera para no forzar cambios
        }

        /// <summary>
        /// Aplica (suma o resta) los valores del informe semanal a la pirámide.
        /// sign = +1 para sumar, -1 para quitar.
        /// (Actualmente no la usamos, pero la dejo por si quieres volver al modo suma/resta)
        /// </summary>
        private static void ApplyWeekly(ref PiramideValues p, InformeSemanalCmaDocument d, int sign)
        {
            p.Seguros += sign * d.ActosSeguros;
            p.Inseguros += sign * d.ActosInseguros;

            p.Precursores1 += sign * d.PrecursoresSifComportamiento;
            p.Precursores2 += sign * d.PrecursoresSifCondicion;

            p.IncidentesSinLesion1 += sign * d.Incidentes;

            p.FAI1 += sign * d.FAI;
            p.MTI1 += sign * d.MTI;
            p.MDI1 += sign * d.MDI;
            p.LTI1 += sign * d.LTI;

            p.Detectadas += sign * d.Detectadas;
            p.Corregidas += sign * d.Corregidas;
        }
    }
}
