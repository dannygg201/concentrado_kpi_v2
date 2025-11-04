using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ConcentradoKPI.App.Models;

namespace ConcentradoKPI.App.ViewModels
{
    public class PersonalVigenteViewModel : INotifyPropertyChanged
    {
        // Contexto
        public Company Company { get; }
        public Project Project { get; }
        public WeekData Week { get; }

        // Encabezado (formulario superior)
        private string? _razonSocial;
        public string? RazonSocial { get => _razonSocial; set { _razonSocial = value; OnPropertyChanged(); } }

        private string? _responsableObra;
        public string? ResponsableObra { get => _responsableObra; set { _responsableObra = value; OnPropertyChanged(); } }

        private string? _registroIMSS;
        public string? RegistroIMSS { get => _registroIMSS; set { _registroIMSS = value; OnPropertyChanged(); } }

        private DateTime? _fecha = DateTime.Today;
        public DateTime? Fecha { get => _fecha; set { _fecha = value; OnPropertyChanged(); } }

        private string? _rfcCompania;
        public string? RFCCompania { get => _rfcCompania; set { _rfcCompania = value; OnPropertyChanged(); } }

        private string? _numeroProveedor;
        public string? NumeroProveedor { get => _numeroProveedor; set { _numeroProveedor = value; OnPropertyChanged(); } }
        private string? _ordenCompra;
        public string? OrdenCompra { get => _ordenCompra; set { _ordenCompra = value; OnPropertyChanged(); } }

        private string? _direccionLegal;
        public string? DireccionLegal { get => _direccionLegal; set { _direccionLegal = value; OnPropertyChanged(); } }

        private string? _observaciones;
        public string? Observaciones { get => _observaciones; set { _observaciones = value; OnPropertyChanged(); } }

        // Nuevo campo Sí/No
        private bool _newEsTecnicoSeguridad;
        public bool NewEsTecnicoSeguridad
        {
            get => _newEsTecnicoSeguridad;
            set { _newEsTecnicoSeguridad = value; OnPropertyChanged(); }
        }

        // Tabla
        public ObservableCollection<PersonRow> Personas { get; } = new();

        // Totales
        public int TotalHH => Personas.Sum(p => p.HHSemana);

        private PersonRow? _selectedPerson;
        public PersonRow? SelectedPerson
        {
            get => _selectedPerson;
            set
            {
                if (_selectedPerson == value) return;
                _selectedPerson = value;
                OnPropertyChanged();

                // Cargar al formulario de edición cuando seleccionas una fila
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
                    // Si se deselecciona todo, limpia el formulario
                    ClearForm();
                }

                // 🔹 Actualiza habilitación de comandos
                RefreshCommands();
            }
        }

        // Campos del formulario (alta/edición)
        private string _newNombre = "";
        public string NewNombre
        {
            get => _newNombre;
            set
            {
                if (_newNombre == value) return;
                _newNombre = value;
                OnPropertyChanged();
                RefreshCommands();
            }
        }

        private string? _newAfiliacion;
        public string? NewAfiliacion
        {
            get => _newAfiliacion;
            set
            {
                if (_newAfiliacion == value) return;
                _newAfiliacion = value;
                OnPropertyChanged();
                RefreshCommands();
            }
        }

        private string? _newPuesto;
        public string? NewPuesto
        {
            get => _newPuesto;
            set
            {
                if (_newPuesto == value) return;
                _newPuesto = value;
                OnPropertyChanged();
                RefreshCommands();
            }
        }

        public int NewD { get => _newD; set { _newD = Pos(value); OnPropertyChanged(); } }
        public int NewL { get => _newL; set { _newL = Pos(value); OnPropertyChanged(); } }
        public int NewM { get => _newM; set { _newM = Pos(value); OnPropertyChanged(); } }
        public int NewMM { get => _newMM; set { _newMM = Pos(value); OnPropertyChanged(); } }
        public int NewJ { get => _newJ; set { _newJ = Pos(value); OnPropertyChanged(); } }
        public int NewV { get => _newV; set { _newV = Pos(value); OnPropertyChanged(); } }
        public int NewS { get => _newS; set { _newS = Pos(value); OnPropertyChanged(); } }

        private int _newD, _newL, _newM, _newMM, _newJ, _newV, _newS;
        private static int Pos(int v) => v < 0 ? 0 : v;

        // Comandos
        public RelayCommand AddPersonCommand { get; }
        public RelayCommand ApplyEditCommand { get; }
        public RelayCommand DeleteSelectedCommand { get; }
        public RelayCommand ClearFormCommand { get; }

        public PersonalVigenteViewModel(Company c, Project p, WeekData w)
        {
            Company = c;
            Project = p;
            Week = w;

            // Cargar datos previos si existen
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
                    r.PropertyChanged += (_, e) =>
                    {
                        if (e.PropertyName == nameof(PersonRow.HHSemana))
                            UpdateTotalsAndLive();
                    };
                    Personas.Add(r);
                }
                UpdateTotalsAndLive();
            }
            else
            {
                UpdateTotalsAndLive();
            }

            Personas.CollectionChanged += (_, __) => RefreshCommands();

            AddPersonCommand = new RelayCommand(_ => AddPerson(), _ => CanAddPerson());
            ApplyEditCommand = new RelayCommand(_ => ApplyEdit(), _ => CanEditOrDelete());
            DeleteSelectedCommand = new RelayCommand(_ => DeleteSelected(), _ => CanEditOrDelete());
            ClearFormCommand = new RelayCommand(_ => ClearForm());
        }

        // --- CanExecute logic ---
        private bool CanAddPerson() => !string.IsNullOrWhiteSpace(NewNombre) && SelectedPerson == null;
        private bool CanEditOrDelete() => SelectedPerson != null;

        private void RefreshCommands()
        {
            AddPersonCommand?.RaiseCanExecuteChanged();
            ApplyEditCommand?.RaiseCanExecuteChanged();
            DeleteSelectedCommand?.RaiseCanExecuteChanged();
        }

        // --- Métodos principales ---
        private void AddPerson()
        {
            // Validación de duplicados (por nombre o afiliación)
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
                return; // 🚫 No agrega duplicado
            }

            // Si pasa validación, crea nuevo registro
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

            // Si cambia HHSemana, recalcula totales
            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(PersonRow.HHSemana)
                    || e.PropertyName == nameof(PersonRow.EsTecnicoSeguridad)) 
                {
                    UpdateTotalsAndLive();
                }
            };


            int insertIndex = GetInsertIndexForPuesto(row.Puesto);
            Personas.Insert(insertIndex, row);
            Renumber();

            UpdateTotalsAndLive();
            ClearForm();
            SelectedPerson = null;

            RefreshCommands();
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

            UpdateTotalsAndLive();
            ClearForm();
            SelectedPerson = null;
            RefreshCommands();
        }

        private void DeleteSelected()
        {
            if (SelectedPerson == null) return;

            Personas.Remove(SelectedPerson);
            Renumber();
            UpdateTotalsAndLive();

            ClearForm();
            SelectedPerson = null;
            RefreshCommands();
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

        private static string NormalizePuesto(string? p)
            => (p ?? "").Trim().ToUpperInvariant();

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

        // Totales en vivo
        private void UpdateTotalsAndLive()
        {
            OnPropertyChanged(nameof(TotalHH));
            if (Week?.Live == null) return;

            Week.Live.ColaboradoresTotal = Personas.Count;
            Week.Live.HorasTrabajadasTotal = Personas.Sum(p => p.HHSemana);
            Week.Live.TecnicosSeguridadTotal = Personas.Count(p => p.EsTecnicoSeguridad);
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
