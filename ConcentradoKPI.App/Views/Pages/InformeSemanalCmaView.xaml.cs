using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.Services;
using ConcentradoKPI.App.ViewModels;
using ConcentradoKPI.App.Views.Abstractions;

namespace ConcentradoKPI.App.Views
{
    public partial class InformeSemanalCmaView : UserControl, ISyncToWeek
    {
        private Company? _company;
        private Project? _project;
        private WeekData? _week;

        private static readonly Regex _onlyDigits = new(@"^\d+$");

        // ===== ctor usado por el diseñador / XAML =====
        public InformeSemanalCmaView()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new InformeSemanalCMAViewModel(
                    semana: "1",
                    proyecto: "Proyecto Demo",
                    nombreInicial: "Contratista Demo",
                    especialidadInicial: "Otros");
            }

            Loaded += OnLoaded;
        }

        // ===== ctor real con contexto =====
        public InformeSemanalCmaView(Company c, Project p, WeekData w) : this()
        {
            _company = c;
            _project = p;
            _week = w;

            DataContext = new InformeSemanalCMAViewModel(
                c, p, w,
                nombreInicial: c?.Name ?? string.Empty,
                especialidadInicial: string.Empty);
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is InformeSemanalCMAViewModel vm)
            {
                // Si no vinieron por ctor, los tomamos del VM
                if (_company == null && _project == null && _week == null)
                {
                    _company = vm.Company;
                    _project = vm.Project;
                    _week = vm.Week;
                }

                vm.Editable.PropertyChanged -= Editable_PropertyChanged;
                vm.Editable.PropertyChanged += Editable_PropertyChanged;
            }

            // Dispara Live para rellenar técnicos/colaboradores/horas
            _week?.Live?.NotifyAll();
        }

        private void Editable_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            ProjectStorageService.MarkDirty();
        }

        // ===== Validación numérica opcional =====
        private void NumericOnly(object sender, TextCompositionEventArgs e)
            => e.Handled = !_onlyDigits.IsMatch(e.Text);

        private void NumericPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject?.GetDataPresent(DataFormats.Text) == true)
            {
                var txt = e.DataObject.GetData(DataFormats.Text)?.ToString() ?? "";
                if (!_onlyDigits.IsMatch(txt)) e.CancelCommand();
            }
            else e.CancelCommand();
        }

        // ======= ISyncToWeek =======
        public void FlushToWeek() => SyncIntoWeek();

        public void SyncIntoWeek()
        {
            if (DataContext is not InformeSemanalCMAViewModel vm) return;
            if (_company is null || _project is null || _week is null) return;

            var e = vm.Editable;

            // 👉 informe semanal ANTERIOR (si lo había)
            var oldWeekly = _week.InformeSemanalCma;

            // 👉 informe semanal NUEVO (lo que ves en pantalla)
            var newWeekly = new InformeSemanalCmaDocument
            {
                Company = _company.Name,
                Project = _project.Name,
                WeekNumber = _week.WeekNumber,

                Nombre = e.Nombre ?? string.Empty,
                Especialidad = e.Especialidad ?? string.Empty,

                // Estos 3 vienen de Live pero se guardan como están
                TecnicosSeguridad = e.TecnicosSeguridad,
                Colaboradores = e.Colaboradores,
                HorasTrabajadas = e.HorasTrabajadas,

                LTI = e.LTI,
                MDI = e.MDI,
                MTI = e.MTI,
                TRI = e.TRI,
                FAI = e.FAI,

                Incidentes = e.Incidentes,

                PrecursoresSifComportamiento = e.PrecursoresSifComportamiento,
                PrecursoresSifCondicion = e.PrecursoresSifCondicion,

                ActosSeguros = e.ActosSeguros,
                ActosInseguros = e.ActosInseguros,

                // Campos nuevos del Excel
                Corregidas = e.Corregidas,
                Detectadas = e.Detectadas,

                TotalSemanal = e.TotalSemanal,
                PorcentajeAvance = e.PorcentajeAvance,

                SavedUtc = DateTime.UtcNow
            };

            // 1) Guardar Informe Semanal de esta semana
            _week.InformeSemanalCma = newWeekly;

            // 2) Recordar última especialidad
            if (!string.IsNullOrWhiteSpace(e.Especialidad))
            {
                LastEspecialidadStore.Set(_company.Name, _project.Name, e.Especialidad);
            }

            // 3) Aplicar la suma n1 + n2 a la pirámide
            if (_week.PiramideSeguridad is PiramideSeguridadDocument pirDoc)
            {
                ApplyWeeklyDeltaToPiramide(pirDoc, oldWeekly, newWeekly);
            }

            ProjectStorageService.MarkDirty();
        }

        // ===== Helpers para sumar sin duplicar =====
        private static int ApplyDelta(int current, int? oldVal, int newVal)
            => current - (oldVal ?? 0) + newVal;

        /// <summary>
        /// pirámideNueva = pirámideActual - oldWeekly + newWeekly
        /// => siempre queda base(n1) + newWeekly(n2)
        /// </summary>
        private static void ApplyWeeklyDeltaToPiramide(
            PiramideSeguridadDocument pir,
            InformeSemanalCmaDocument? oldWeekly,
            InformeSemanalCmaDocument newWeekly)
        {
            // LTI / MDI / MTI / FAI -> usamos nivel 1
            pir.FAI1 = ApplyDelta(pir.FAI1, oldWeekly?.FAI, newWeekly.FAI);
            pir.MTI1 = ApplyDelta(pir.MTI1, oldWeekly?.MTI, newWeekly.MTI);
            pir.MDI1 = ApplyDelta(pir.MDI1, oldWeekly?.MDI, newWeekly.MDI);
            pir.LTI1 = ApplyDelta(pir.LTI1, oldWeekly?.LTI, newWeekly.LTI);

            // Incidentes sin lesión
            pir.IncidentesSinLesion1 = ApplyDelta(
                pir.IncidentesSinLesion1,
                oldWeekly?.Incidentes,
                newWeekly.Incidentes);

            // Precursores SIF
            pir.Precursores1 = ApplyDelta(
                pir.Precursores1,
                oldWeekly?.PrecursoresSifComportamiento,
                newWeekly.PrecursoresSifComportamiento);

            pir.Precursores2 = ApplyDelta(
                pir.Precursores2,
                oldWeekly?.PrecursoresSifCondicion,
                newWeekly.PrecursoresSifCondicion);

            // Actos
            pir.Seguros = ApplyDelta(
                pir.Seguros,
                oldWeekly?.ActosSeguros,
                newWeekly.ActosSeguros);

            pir.Inseguros = ApplyDelta(
                pir.Inseguros,
                oldWeekly?.ActosInseguros,
                newWeekly.ActosInseguros);

            // Condiciones (campos del Excel)
            pir.Corregidas = ApplyDelta(
                pir.Corregidas,
                oldWeekly?.Corregidas,
                newWeekly.Corregidas);

            pir.Detectadas = ApplyDelta(
                pir.Detectadas,
                oldWeekly?.Detectadas,
                newWeekly.Detectadas);
        }
    }
}
