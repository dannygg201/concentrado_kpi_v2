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
        // Contexto que llega desde MainWindow
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

        private string? _direccionLegal;
        public string? DireccionLegal { get => _direccionLegal; set { _direccionLegal = value; OnPropertyChanged(); } }

        // Tabla
        public ObservableCollection<PersonRow> Personas { get; } = new();

        private PersonRow? _selectedPerson;
        public PersonRow? SelectedPerson
        {
            get => _selectedPerson;
            set
            {
                if (_selectedPerson == value) return;
                _selectedPerson = value;
                OnPropertyChanged();
                // Cargar al formulario de edición
                if (value != null)
                {
                    NewNombre = value.Nombre;
                    NewAfiliacion = value.Afiliacion;
                    NewPuesto = value.Puesto;
                    NewD = value.D; NewL = value.L; NewM = value.M; NewMM = value.MM; NewJ = value.J; NewV = value.V; NewS = value.S;
                }
                AddPersonCommand?.RaiseCanExecuteChanged();
                ApplyEditCommand?.RaiseCanExecuteChanged();
                DeleteSelectedCommand?.RaiseCanExecuteChanged();
            }
        }



        // Totales
        public int TotalHH => Personas.Sum(p => p.HHSemana);

        // Campos de alta/edición (parte inferior)
        private string _newNombre = "";
        public string NewNombre
        {
            get => _newNombre;
            set
            {
                if (_newNombre == value) return;
                _newNombre = value;
                OnPropertyChanged();
                AddPersonCommand?.RaiseCanExecuteChanged();
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
                AddPersonCommand?.RaiseCanExecuteChanged();
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
                AddPersonCommand?.RaiseCanExecuteChanged();
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

            var doc = w.PersonalVigente;
            if (doc != null)
            {
                RazonSocial = doc.RazonSocial;
                ResponsableObra = doc.ResponsableObra;
                RegistroIMSS = doc.RegistroIMSS;
                RFCCompania = doc.RFCCompania;
                DireccionLegal = doc.DireccionLegal;
                NumeroProveedor = doc.NumeroProveedor;
                Fecha = doc.Fecha;

                Personas.Clear();
                foreach (var r in doc.Personal ?? Enumerable.Empty<PersonRow>())
                    Personas.Add(r);
            }
        }

        private void AddPerson()
        {
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
                S = NewS
            };

            // Escuchar cambios de la fila para refrescar TotalHH
            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(PersonRow.HHSemana))
                    OnPropertyChanged(nameof(TotalHH));
            };

            int insertIndex = GetInsertIndexForPuesto(row.Puesto);
            Personas.Insert(insertIndex, row);
            Renumber();
            OnPropertyChanged(nameof(TotalHH));
            ClearForm();
            SelectedPerson = null;
            // refresca botones
            AddPersonCommand?.RaiseCanExecuteChanged();
            ApplyEditCommand?.RaiseCanExecuteChanged();
            DeleteSelectedCommand?.RaiseCanExecuteChanged();
        }

        private void ApplyEdit()
        {
            if (SelectedPerson == null) return;

            var row = SelectedPerson;

            // ¿cambió el puesto?
            string oldPuestoNorm = NormalizePuesto(row.Puesto);
            string newPuestoNorm = NormalizePuesto(NewPuesto);

            row.Nombre = NewNombre.Trim();
            row.Afiliacion = NewAfiliacion;
            row.Puesto = NewPuesto;
            row.D = NewD; row.L = NewL; row.M = NewM;
            row.MM = NewMM; row.J = NewJ; row.V = NewV; row.S = NewS;

            // Si cambió, mover a su grupo
            if (oldPuestoNorm != newPuestoNorm)
            {
                // lo quitamos y reinsertamos en el índice correcto
                Personas.Remove(row);
                int insertIndex = GetInsertIndexForPuesto(row.Puesto);
                Personas.Insert(insertIndex, row);
                Renumber();
            }

            // refresca total y limpia
            OnPropertyChanged(nameof(TotalHH));
            ClearForm();
            SelectedPerson = null;
            AddPersonCommand?.RaiseCanExecuteChanged();
            ApplyEditCommand?.RaiseCanExecuteChanged();
            DeleteSelectedCommand?.RaiseCanExecuteChanged();

        }

        private void DeleteSelected()
        {
            if (SelectedPerson == null) return;

            Personas.Remove(SelectedPerson);
            Renumber();
           
            OnPropertyChanged(nameof(TotalHH));

            // Limpia y deselecciona (no volvemos a cargar nada al formulario)
            ClearForm();
            SelectedPerson = null;
            // refresca botones
            AddPersonCommand?.RaiseCanExecuteChanged();       
            ApplyEditCommand?.RaiseCanExecuteChanged();
            DeleteSelectedCommand?.RaiseCanExecuteChanged();
        }

        private void ClearForm()
        {
            NewNombre = "";
            NewAfiliacion = "";
            NewPuesto = "";
            NewD = NewL = NewM = NewMM = NewJ = NewV = NewS = 0;

            AddPersonCommand?.RaiseCanExecuteChanged();
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

            // Busca el último índice de ese puesto (ignorando 'ignore' si se pasa)
            int lastIndex = -1;
            for (int i = 0; i < Personas.Count; i++)
            {
                if (Personas[i] == ignore) continue;
                if (NormalizePuesto(Personas[i].Puesto) == target)
                    lastIndex = i;
            }

            // Si lo encontró, inserta después de ese último.
            if (lastIndex >= 0) return lastIndex + 1;

            // Si no existe ese puesto aún, al final.
            return Personas.Count;
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
