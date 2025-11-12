using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.Services;

namespace ConcentradoKPI.App.ViewModels
{
    public class PrecursorSifViewModel : INotifyPropertyChanged
    {
        // TopBar
        public Company Company { get; }
        public Project Project { get; }
        public WeekData Week { get; }

        public string EncabezadoDescripcion { get; }

        public ObservableCollection<PrecursorSifRecord> Registros { get; } = new();

        private PrecursorSifRecord? _seleccionado;
        public PrecursorSifRecord? Seleccionado
        {
            get => _seleccionado;
            set
            {
                if (_seleccionado == value) return;
                _seleccionado = value;
                OnPropertyChanged();
                CargarEnFormulario(value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        // Formulario editable
        private PrecursorSifRecord _form = new();
        public PrecursorSifRecord Form
        {
            get => _form;
            set
            {
                if (_form == value) return;
                _form = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        // Opciones
        public string[] PrecursorSifOpciones { get; } = { "Precursor", "Potencial", "Real" };
        public string[] TipoPrecursorOpciones { get; } = { "Comportamiento", "Condición" };

        // Comandos
        public ICommand AgregarCmd { get; }
        public ICommand GuardarCmd { get; }
        public ICommand EliminarCmd { get; }

        public int TotalRegistros => Registros.Count;

        // Runtime
        public PrecursorSifViewModel(Company c, Project p, WeekData w)
        {
            Company = c;
            Project = p;
            Week = w;

            EncabezadoDescripcion = $"Semana {w.WeekNumber}  |  {p.Name}";

            AgregarCmd = new RelayCommand(_ => Agregar(), _ => PuedeAgregar() && Seleccionado == null);
            GuardarCmd = new RelayCommand(_ => Guardar(), _ => PuedeAgregar() && Seleccionado != null);
            EliminarCmd = new RelayCommand(_ => Eliminar(), _ => Seleccionado != null);

            Registros.CollectionChanged += (_, __) => OnPropertyChanged(nameof(TotalRegistros));

            if (string.IsNullOrWhiteSpace(Form.UEN))
                Form.UEN = "CMC";
        }

        // Diseño/prueba
        public PrecursorSifViewModel(string semana, string proyecto)
        {
            Company = new Company { Name = "Demo Co." };
            Project = new Project { Name = string.IsNullOrWhiteSpace(proyecto) ? "Proyecto Demo" : proyecto };
            Week = new WeekData { WeekNumber = int.TryParse(semana, out var n) ? n : 1 };

            EncabezadoDescripcion = $"Semana {semana}  |  {proyecto}";

            AgregarCmd = new RelayCommand(_ => Agregar(), _ => PuedeAgregar() && Seleccionado == null);
            GuardarCmd = new RelayCommand(_ => Guardar(), _ => PuedeAgregar() && Seleccionado != null);
            EliminarCmd = new RelayCommand(_ => Eliminar(), _ => Seleccionado != null);

            Registros.CollectionChanged += (_, __) => OnPropertyChanged(nameof(TotalRegistros));

            if (string.IsNullOrWhiteSpace(Form.UEN))
                Form.UEN = "CMC";
        }

        private bool PuedeAgregar()
        {
            return !string.IsNullOrWhiteSpace(Form.NombreInvolucrado)
                && !string.IsNullOrWhiteSpace(Form.CompaniaContratista)
                && !string.IsNullOrWhiteSpace(Form.PrecursorSif)
                && !string.IsNullOrWhiteSpace(Form.TipoPrecursor);
        }

        private void Agregar()
        {
            if (string.IsNullOrWhiteSpace(Form.UEN))
                Form.UEN = "CMC";

            var nuevo = Form.Clone();
            nuevo.No = Registros.Count + 1;
            Registros.Add(nuevo);

            // limpiar y des-seleccionar
            Seleccionado = null;
            Form = new PrecursorSifRecord { UEN = "CMC" };
            ProjectStorageService.MarkDirty();
        }

        private void Guardar()
        {
            if (Seleccionado is null) return;

            Seleccionado.CopyFrom(Form);

            // limpiar y des-seleccionar
            Seleccionado = null;
            Form = new PrecursorSifRecord { UEN = "CMC" };
            ProjectStorageService.MarkDirty();
        }

        private void Eliminar()
        {
            if (Seleccionado is null) return;

            var idx = Registros.IndexOf(Seleccionado);
            if (idx >= 0) Registros.RemoveAt(idx);
            Reenumerar();

            // limpiar y des-seleccionar
            Seleccionado = null;
            Form = new PrecursorSifRecord { UEN = "CMC" };
            ProjectStorageService.MarkDirty();
        }

        private void Reenumerar()
        {
            for (int i = 0; i < Registros.Count; i++)
                Registros[i].No = i + 1;
        }

        private void CargarEnFormulario(PrecursorSifRecord? r)
        {
            if (r == null)
            {
                // modo “agregar”
                Form = new PrecursorSifRecord { UEN = "CMC" };
                return;
            }

            Form = r.Clone();
            if (string.IsNullOrWhiteSpace(Form.UEN))
                Form.UEN = "CMC";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}
