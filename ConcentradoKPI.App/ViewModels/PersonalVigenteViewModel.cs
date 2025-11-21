using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Threading;
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.Services;

namespace ConcentradoKPI.App.ViewModels
{
    public class PersonalVigenteViewModel : INotifyPropertyChanged, IDisposable
    {
        // ===== Contexto =====
        public Company Company { get; }
        public Project Project { get; }
        public WeekData Week { get; }

        // ===== Encabezado =====
        private string? _razonSocial;
        public string? RazonSocial { get => _razonSocial; set { if (_razonSocial == value) return; _razonSocial = value; OnPropertyChanged(); } }

        private string? _responsableObra;
        public string? ResponsableObra { get => _responsableObra; set { if (_responsableObra == value) return; _responsableObra = value; OnPropertyChanged(); } }

        private string? _registroIMSS;
        public string? RegistroIMSS { get => _registroIMSS; set { if (_registroIMSS == value) return; _registroIMSS = value; OnPropertyChanged(); } }

        private DateTime? _fecha = DateTime.Today;
        public DateTime? Fecha { get => _fecha; set { if (_fecha == value) return; _fecha = value; OnPropertyChanged(); } }

        private string? _rfcCompania;
        public string? RFCCompania { get => _rfcCompania; set { if (_rfcCompania == value) return; _rfcCompania = value; OnPropertyChanged(); } }

        private string? _numeroProveedor;
        public string? NumeroProveedor { get => _numeroProveedor; set { if (_numeroProveedor == value) return; _numeroProveedor = value; OnPropertyChanged(); } }

        private string? _ordenCompra;
        public string? OrdenCompra { get => _ordenCompra; set { if (_ordenCompra == value) return; _ordenCompra = value; OnPropertyChanged(); } }

        private string? _direccionLegal;
        public string? DireccionLegal { get => _direccionLegal; set { if (_direccionLegal == value) return; _direccionLegal = value; OnPropertyChanged(); } }

        private string? _observaciones;
        public string? Observaciones { get => _observaciones; set { if (_observaciones == value) return; _observaciones = value; OnPropertyChanged(); } }

        // Flag Tec. Seguridad (formulario)
        private bool _newEsTecnicoSeguridad;
        public bool NewEsTecnicoSeguridad
        {
            get => _newEsTecnicoSeguridad;
            set { if (_newEsTecnicoSeguridad == value) return; _newEsTecnicoSeguridad = value; OnPropertyChanged(); }
        }

        // ===== Tabla =====
        public ObservableCollection<PersonRow> Personas { get; } = new();

        // ✅ Totales calculados desde D..S (no dependas de HHSemana)
        public int TotalHH => Personas.Sum(p => p.D + p.L + p.M + p.MM + p.J + p.V + p.S);

        private PersonRow? _selectedPerson;
        public PersonRow? SelectedPerson
        {
            get => _selectedPerson;
            set
            {
                if (_selectedPerson == value) return;
                _selectedPerson = value;
                OnPropertyChanged();

                if (value != null)
                {
                    NewNombre = value.Nombre;
                    NewAfiliacion = value.Afiliacion;
                    NewPuesto = value.Puesto;
                    NewD = value.D; NewL = value.L; NewM = value.M; NewMM = value.MM;
                    NewJ = value.J; NewV = value.V; NewS = value.S;
                    NewEsTecnicoSeguridad = value.EsTecnicoSeguridad;
                }
                else
                {
                    ClearForm();
                }

                RefreshCommands();
            }
        }

        // ===== Campos del formulario =====
        private string _newNombre = "";
        public string NewNombre { get => _newNombre; set { if (_newNombre == value) return; _newNombre = value; OnPropertyChanged(); RefreshCommands(); } }

        private string? _newAfiliacion;
        public string? NewAfiliacion { get => _newAfiliacion; set { if (_newAfiliacion == value) return; _newAfiliacion = value; OnPropertyChanged(); RefreshCommands(); } }

        private string? _newPuesto;
        public string? NewPuesto { get => _newPuesto; set { if (_newPuesto == value) return; _newPuesto = value; OnPropertyChanged(); RefreshCommands(); } }

        public int NewD { get => _newD; set { var nv = Pos(value); if (_newD == nv) return; _newD = nv; OnPropertyChanged(); } }
        public int NewL { get => _newL; set { var nv = Pos(value); if (_newL == nv) return; _newL = nv; OnPropertyChanged(); } }
        public int NewM { get => _newM; set { var nv = Pos(value); if (_newM == nv) return; _newM = nv; OnPropertyChanged(); } }
        public int NewMM { get => _newMM; set { var nv = Pos(value); if (_newMM == nv) return; _newMM = nv; OnPropertyChanged(); } }
        public int NewJ { get => _newJ; set { var nv = Pos(value); if (_newJ == nv) return; _newJ = nv; OnPropertyChanged(); } }
        public int NewV { get => _newV; set { var nv = Pos(value); if (_newV == nv) return; _newV = nv; OnPropertyChanged(); } }
        public int NewS { get => _newS; set { var nv = Pos(value); if (_newS == nv) return; _newS = nv; OnPropertyChanged(); } }

        private int _newD, _newL, _newM, _newMM, _newJ, _newV, _newS;
        private static int Pos(int v) => v < 0 ? 0 : v;

        // ===== Comandos =====
        public RelayCommand AddPersonCommand { get; }
        public RelayCommand ApplyEditCommand { get; }
        public RelayCommand DeleteSelectedCommand { get; }
        public RelayCommand ClearFormCommand { get; }

        private bool _isPushingToWeek;

        // ===== Throttle / Guards =====
        private int _totalsScheduled;     // 0 = no agendado, 1 = agendado
        private bool _isUpdatingTotals;   // guard reentrante

        public PersonalVigenteViewModel(Company c, Project p, WeekData w)
        {
            Company = c;
            Project = p;
            Week = w;

            // Hidrata si ya hay documento
            var doc = w.PersonalVigenteDocument;
            if (doc != null)
            {
                RazonSocial = doc.RazonSocial;
                ResponsableObra = doc.ResponsableObra;
                RegistroIMSS = doc.RegistroIMSS;
                RFCCompania = doc.RFCCompania;
                DireccionLegal = doc.DireccionLegal;
                NumeroProveedor = doc.NumeroProveedor;
                OrdenCompra = doc.OrdenCompra;
                Fecha = doc.Fecha;
                Observaciones = doc.Observaciones;

                Personas.Clear();
                foreach (var r in doc.Personal ?? Enumerable.Empty<PersonRow>())
                {
                    r.PropertyChanged += OnPersonRowPropertyChanged;
                    Personas.Add(r);
                }
                ScheduleTotals();
            }
            else
            {
                ScheduleTotals();
            }

            Personas.CollectionChanged += Personas_CollectionChanged;

            AddPersonCommand = new RelayCommand(_ => AddPerson(), _ => CanAddPerson());
            ApplyEditCommand = new RelayCommand(_ => ApplyEdit(), _ => CanEditOrDelete());
            DeleteSelectedCommand = new RelayCommand(_ => DeleteSelected(), _ => CanEditOrDelete());
            ClearFormCommand = new RelayCommand(_ => ClearForm());
        }

        // ===== Handlers =====
        private void OnPersonRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PersonRow.HHSemana) ||
                e.PropertyName == nameof(PersonRow.EsTecnicoSeguridad) ||
                e.PropertyName == nameof(PersonRow.D) ||
                e.PropertyName == nameof(PersonRow.L) ||
                e.PropertyName == nameof(PersonRow.M) ||
                e.PropertyName == nameof(PersonRow.MM) ||
                e.PropertyName == nameof(PersonRow.J) ||
                e.PropertyName == nameof(PersonRow.V) ||
                e.PropertyName == nameof(PersonRow.S))
            {
                ScheduleTotals();
            }
        }

        private void Personas_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
                foreach (PersonRow r in e.NewItems) r.PropertyChanged += OnPersonRowPropertyChanged;

            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
                foreach (PersonRow r in e.OldItems) r.PropertyChanged -= OnPersonRowPropertyChanged;

            if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                if (e.OldItems != null) foreach (PersonRow r in e.OldItems) r.PropertyChanged -= OnPersonRowPropertyChanged;
                if (e.NewItems != null) foreach (PersonRow r in e.NewItems) r.PropertyChanged += OnPersonRowPropertyChanged;
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var r in Personas) { r.PropertyChanged -= OnPersonRowPropertyChanged; r.PropertyChanged += OnPersonRowPropertyChanged; }
            }

            Renumber();
            ScheduleTotals();
            RefreshCommands();
        }

        // ===== CanExecute =====
        private bool CanAddPerson() => !string.IsNullOrWhiteSpace(NewNombre) && SelectedPerson == null;
        private bool CanEditOrDelete() => SelectedPerson != null;

        private void RefreshCommands()
        {
            AddPersonCommand?.RaiseCanExecuteChanged();
            ApplyEditCommand?.RaiseCanExecuteChanged();
            DeleteSelectedCommand?.RaiseCanExecuteChanged();
        }

        // ===== CRUD filas =====
        private void AddPerson()
        {
            bool existeDuplicado = Personas.Any(p =>
                string.Equals(p.Nombre?.Trim(), NewNombre?.Trim(), StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(NewAfiliacion) &&
                 string.Equals(p.Afiliacion?.Trim(), NewAfiliacion?.Trim(), StringComparison.OrdinalIgnoreCase))
            );

            if (existeDuplicado)
            {
                System.Windows.MessageBox.Show(
                    "Ya existe una persona con el mismo nombre o número de afiliación.",
                    "Duplicado detectado",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning
                );
                return;
            }

            var row = new PersonRow
            {
                Numero = Personas.Count + 1,
                Nombre = NewNombre.Trim(),
                Afiliacion = NewAfiliacion,
                Puesto = NewPuesto,
                D = NewD,
                L = NewL,
                M = NewM,
                MM = NewMM,
                J = NewJ,
                V = NewV,
                S = NewS,
                EsTecnicoSeguridad = NewEsTecnicoSeguridad
            };

            row.PropertyChanged += OnPersonRowPropertyChanged;

            int insertIndex = GetInsertIndexForPuesto(row.Puesto);
            Personas.Insert(insertIndex, row);
            Renumber();

            ScheduleTotals();
            ClearForm();
            SelectedPerson = null;
            RefreshCommands();
            ProjectStorageService.MarkDirty();
        }

        private void ApplyEdit()
        {
            if (SelectedPerson == null) return;

            var row = SelectedPerson;
            string oldPuestoNorm = NormalizePuesto(row.Puesto);
            string newPuestoNorm = NormalizePuesto(NewPuesto);

            row.Nombre = NewNombre.Trim();
            row.Afiliacion = NewAfiliacion;
            row.Puesto = NewPuesto;
            row.D = NewD; row.L = NewL; row.M = NewM; row.MM = NewMM;
            row.J = NewJ; row.V = NewV; row.S = NewS;
            row.EsTecnicoSeguridad = NewEsTecnicoSeguridad;

            if (oldPuestoNorm != newPuestoNorm)
            {
                Personas.Remove(row);
                int insertIndex = GetInsertIndexForPuesto(row.Puesto);
                Personas.Insert(insertIndex, row);
                Renumber();
            }

            ScheduleTotals();
            ClearForm();
            SelectedPerson = null;
            RefreshCommands();
            ProjectStorageService.MarkDirty();
        }

        private void DeleteSelected()
        {
            if (SelectedPerson == null) return;

            Personas.Remove(SelectedPerson);
            Renumber();
            ScheduleTotals();

            ClearForm();
            SelectedPerson = null;
            RefreshCommands();
            ProjectStorageService.MarkDirty();
        }

        private void ClearForm()
        {
            NewNombre = "";
            NewAfiliacion = "";
            NewPuesto = "";
            NewD = NewL = NewM = NewMM = NewJ = NewV = NewS = 0;
            NewEsTecnicoSeguridad = false;
            RefreshCommands();
        }

        private void Renumber()
        {
            for (int i = 0; i < Personas.Count; i++)
                Personas[i].Numero = i + 1;
        }

        private static string NormalizePuesto(string? p) => (p ?? "").Trim().ToUpperInvariant();

        private int GetInsertIndexForPuesto(string? puesto, PersonRow? ignore = null)
        {
            string target = NormalizePuesto(puesto);
            int lastIndex = -1;
            for (int i = 0; i < Personas.Count; i++)
            {
                if (Personas[i] == ignore) continue;
                if (NormalizePuesto(Personas[i].Puesto) == target)
                    lastIndex = i;
            }
            return lastIndex >= 0 ? lastIndex + 1 : Personas.Count;
        }

        // ===== Live (fuente de verdad para Pirámide) =====
        private void ScheduleTotals()
        {
            // Si ya hay una ejecución pendiente, no agendas otra.
            if (Interlocked.Exchange(ref _totalsScheduled, 1) == 1) return;

            System.Windows.Application.Current?.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    try { UpdateTotalsAndLiveInternal(); }
                    finally { Interlocked.Exchange(ref _totalsScheduled, 0); }
                }),
                DispatcherPriority.Background);
        }

        private void UpdateTotalsAndLiveInternal()
        {
            if (_isUpdatingTotals) return;
            _isUpdatingTotals = true;
            try
            {
                OnPropertyChanged(nameof(TotalHH));

                if (Week?.Live != null)
                {
                    Week.Live.ColaboradoresTotal = Personas.Count;
                    Week.Live.TecnicosSeguridadTotal = Personas.Count(p => p.EsTecnicoSeguridad);
                    Week.Live.HorasTrabajadasTotal = Personas.Sum(p => p.D + p.L + p.M + p.MM + p.J + p.V + p.S);
                }
            }
            finally { _isUpdatingTotals = false; }
        }

        // ===== Persistencia =====
        public void PushToWeekData()
        {
            if (Week == null) return;
            if (_isPushingToWeek) return;
            _isPushingToWeek = true;

            try
            {
                var doc = new PersonalVigenteDocument
                {
                    Company = Company?.Name,
                    Project = Project?.Name,
                    WeekNumber = Week.WeekNumber,
                    RazonSocial = RazonSocial,
                    ResponsableObra = ResponsableObra,
                    RegistroIMSS = RegistroIMSS,
                    RFCCompania = RFCCompania,
                    DireccionLegal = DireccionLegal,
                    NumeroProveedor = NumeroProveedor,
                    OrdenCompra = OrdenCompra,
                    Fecha = Fecha,
                    Observaciones = Observaciones,
                    Personal = Personas.ToList(),
                    SavedUtc = DateTime.UtcNow
                };

                Week.PersonalVigenteDocument = doc;

                // Recalcular Live a partir del documento recién persistido (sin NotifyAll)
                LiveSyncService.Recalc(Week, doc);
            }
            finally
            {
                _isPushingToWeek = false;
            }
        }

        // ===== INotifyPropertyChanged =====
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        // ===== Limpieza =====
        public void Dispose()
        {
            Personas.CollectionChanged -= Personas_CollectionChanged;
            foreach (var r in Personas)
                r.PropertyChanged -= OnPersonRowPropertyChanged;
        }
    }
}
