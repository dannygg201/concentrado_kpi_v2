using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;


namespace ConcentradoKPI.App.ViewModels
{
    public class IncidentesViewModel : INotifyPropertyChanged
    {
        public Company Company { get; }
        public Project Project { get; }
        public WeekData Week { get; }

        public string EncabezadoDescripcion { get; }
        public ObservableCollection<IncidentRecord> Registros { get; } = new();

        private IncidentRecord? _seleccionado;
        public IncidentRecord? Seleccionado
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

        private IncidentRecord _form = new();
        public IncidentRecord Form
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

        public string[] Clasificaciones { get; } = { "LTI", "MDI", "MTI", "FAI", "INCIDENTE" };

        public ICommand AgregarCmd { get; }
        public ICommand GuardarCmd { get; }
        public ICommand EliminarCmd { get; }

        public int TotalRegistros => Registros.Count;

        public IncidentesViewModel(Company c, Project p, WeekData w)
        {
            Company = c; Project = p; Week = w;
            EncabezadoDescripcion = $"Semana {w.WeekNumber}  |  {p.Name}";

            AgregarCmd = new RelayCommand(_ => Agregar(), _ => PuedeAgregar() && Seleccionado == null);
            GuardarCmd = new RelayCommand(_ => Guardar(), _ => PuedeAgregar() && Seleccionado != null);
            EliminarCmd = new RelayCommand(_ => Eliminar(), _ => Seleccionado != null);

            Registros.CollectionChanged += (_, __) => OnPropertyChanged(nameof(TotalRegistros));

            if (string.IsNullOrWhiteSpace(Form.UEN)) Form.UEN = "CMC";
        }

        private bool PuedeAgregar()
        {
            return !string.IsNullOrWhiteSpace(Form.NombreInvolucrado)
                && !string.IsNullOrWhiteSpace(Form.CompaniaContratista)
                && !string.IsNullOrWhiteSpace(Form.Clasificacion);
        }

        private void Agregar()
        {
            if (string.IsNullOrWhiteSpace(Form.UEN)) Form.UEN = "CMC";

            var nuevo = Form.Clone();
            nuevo.No = Registros.Count + 1;
            Registros.Add(nuevo);

            // Limpia y des-selecciona
            Seleccionado = null;
            Form = new IncidentRecord { UEN = "CMC" };
            ProjectStorageService.MarkDirty();
        }

        private void Guardar()
        {
            if (Seleccionado is null) return;

            Seleccionado.CopyFrom(Form);

            // Limpia y des-selecciona
            Seleccionado = null;
            Form = new IncidentRecord { UEN = "CMC" };
            ProjectStorageService.MarkDirty();
        }

        private void Eliminar()
        {
            if (Seleccionado is null) return;
            var idx = Registros.IndexOf(Seleccionado);
            if (idx >= 0) Registros.RemoveAt(idx);
            Reenumerar();

            // Limpia y des-selecciona
            Seleccionado = null;
            Form = new IncidentRecord { UEN = "CMC" };
            ProjectStorageService.MarkDirty();
        }

        private void Reenumerar()
        {
            for (int i = 0; i < Registros.Count; i++) Registros[i].No = i + 1;
        }

        private void CargarEnFormulario(IncidentRecord? r)
        {
            if (r == null)
            {
                // Modo “agregar”
                Form = new IncidentRecord { UEN = "CMC" };
                return;
            }
            Form = r.Clone();
            if (string.IsNullOrWhiteSpace(Form.UEN)) Form.UEN = "CMC";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}
