using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.Services;
using ConcentradoKPI.App.Views;
using Microsoft.Win32;                // OpenFileDialog/SaveFileDialog
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO; 
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;                 // MessageBox
using System.Windows.Data;

namespace ConcentradoKPI.App.ViewModels
{
    public enum ExportScope { All, Company, Project, OneWeek, ManyWeeks }

    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Company> Companies { get; } = new();
        public AppData App { get; private set; } = new AppData();

        // === Navegación por módulos (para el NavBar + ContentControl) ===
        private AppView _currentView = AppView.PersonalVigente;
        public AppView CurrentView
        {
            get => _currentView;
            set
            {
                if (_currentView != value)
                {
                    _currentView = value;
                    OnPropertyChanged();
                }
            }
        }

        public RelayCommand NavigateCommand { get; }


        private Company? _selectedCompany;
        public Company? SelectedCompany
        {
            get => _selectedCompany;
            set
            {
                _selectedCompany = value;
                OnPropertyChanged();
                AddProjectCommand?.RaiseCanExecuteChanged();
                AddWeekCommand?.RaiseCanExecuteChanged();

                RenameCompanyCommand?.RaiseCanExecuteChanged();
                RenameProjectCommand?.RaiseCanExecuteChanged();
                RenameWeekCommand?.RaiseCanExecuteChanged();

                DeleteCompanyCommand?.RaiseCanExecuteChanged();
                DeleteProjectCommand?.RaiseCanExecuteChanged();
                DeleteWeekCommand?.RaiseCanExecuteChanged();

                ExportCommand?.RaiseCanExecuteChanged();
                OpenPersonalCommand?.RaiseCanExecuteChanged();

            }
        }

        private Project? _selectedProject;
        public Project? SelectedProject
        {
            get => _selectedProject;
            set
            {
                _selectedProject = value;
                OnPropertyChanged();

                AddWeekCommand?.RaiseCanExecuteChanged();

                RenameCompanyCommand?.RaiseCanExecuteChanged();
                RenameProjectCommand?.RaiseCanExecuteChanged();
                RenameWeekCommand?.RaiseCanExecuteChanged();

                DeleteCompanyCommand?.RaiseCanExecuteChanged();
                DeleteProjectCommand?.RaiseCanExecuteChanged();
                DeleteWeekCommand?.RaiseCanExecuteChanged();
                OpenPersonalCommand?.RaiseCanExecuteChanged();

            }
        }

        private WeekData? _selectedWeek;
        public WeekData? SelectedWeek
        {
            get => _selectedWeek;
            set {
                _selectedWeek = value;
                OnPropertyChanged();

                RenameWeekCommand?.RaiseCanExecuteChanged();
                DeleteWeekCommand?.RaiseCanExecuteChanged();
                OpenPersonalCommand?.RaiseCanExecuteChanged();

            }
        }

        public string Mensaje { get; set; } = "Selecciona o crea compañías, proyectos y semanas.";

        // Comandos
        public RelayCommand AddCompanyCommand { get; }
        public RelayCommand AddProjectCommand { get; }
        public RelayCommand AddWeekCommand { get; }
        public RelayCommand ImportCommand { get; }
        public RelayCommand ExportCommand { get; }

        public RelayCommand RenameCompanyCommand { get; }
        public RelayCommand RenameProjectCommand { get; }
        public RelayCommand RenameWeekCommand { get; }

        public RelayCommand DeleteCompanyCommand { get; }
        public RelayCommand DeleteProjectCommand { get; }
        public RelayCommand DeleteWeekCommand { get; }

        // Evento que la Vista escuchará para abrir la ventana de Personal Vigente
        public event Action<Company, Project, WeekData>? OpenPersonalRequested;

        // Comando para abrir Personal Vigente (solo si hay semana seleccionada)
        public RelayCommand OpenPersonalCommand { get; }

        public RelayCommand NewCommand { get; }
        public MainViewModel()
        {
         
            AddCompanyCommand = new RelayCommand(_ => AddCompany());
            AddProjectCommand = new RelayCommand(_ => AddProject(), _ => SelectedCompany != null);
            AddWeekCommand = new RelayCommand(_ => AddWeek(), _ => SelectedProject != null);

            ImportCommand = new RelayCommand(async _ => await ImportAsync());
            ExportCommand = new RelayCommand(async _ => await ExportAsync(), _ => Companies.Any());

            RenameCompanyCommand = new RelayCommand(_ => RenameCompany(), _ => SelectedCompany != null);
            RenameProjectCommand = new RelayCommand(_ => RenameProject(), _ => SelectedProject != null);
            RenameWeekCommand = new RelayCommand(_ => RenameWeek(), _ => SelectedWeek != null);

            DeleteCompanyCommand = new RelayCommand(_ => DeleteCompany(), _ => SelectedCompany != null);
            DeleteProjectCommand = new RelayCommand(_ => DeleteProject(), _ => SelectedProject != null);
            DeleteWeekCommand = new RelayCommand(_ => DeleteWeek(), _ => SelectedWeek != null);
            NewCommand = new RelayCommand(_ => NewDocument());
            // Navegación por enum desde el NavBar
            NavigateCommand = new RelayCommand(p =>
            {
                if (p is AppView v)
                    CurrentView = v;
            });


            OpenPersonalCommand = new RelayCommand(
           _ => OpenPersonal(),
           _ => SelectedCompany != null && SelectedProject != null && SelectedWeek != null
       );



            RefreshCommandStates();
       

        }
        private void NewDocument()
        {
            App = new AppData();
            Companies.Clear();

            // 🔑 sin ruta => Guardar pedirá ubicación
            ProjectStorageService.Bind(App, path: null);

            SelectedCompany = null;
            SelectedProject = null;
            SelectedWeek = null;
            Mensaje = "Documento nuevo.";
            OnPropertyChanged(nameof(Mensaje));
        }
        private void RefreshCommandStates()
        {
            AddProjectCommand?.RaiseCanExecuteChanged();
            AddWeekCommand?.RaiseCanExecuteChanged();

            RenameCompanyCommand?.RaiseCanExecuteChanged();
            RenameProjectCommand?.RaiseCanExecuteChanged();
            RenameWeekCommand?.RaiseCanExecuteChanged();

            DeleteCompanyCommand?.RaiseCanExecuteChanged();
            DeleteProjectCommand?.RaiseCanExecuteChanged();
            DeleteWeekCommand?.RaiseCanExecuteChanged();

            ExportCommand?.RaiseCanExecuteChanged();

            // Documento nuevo: datos vivos = App, sin ruta -> al dar Guardar pedirá ubicación
            ProjectStorageService.Bind(App, path: null);

        }

        private void RefreshTree()
        {
            // Fuerza a WPF a re-renderizar el TreeView después de renombrar
            CollectionViewSource.GetDefaultView(Companies)?.Refresh();
        }

        // ====== Agregar ======
        private void AddCompany()
        {
            var name = AskText("Nueva compañía", "Nombre de la compañía:", $"Compañía {Companies.Count + 1}");
            if (name == null) return;

            bool creatingFirst = Companies.Count == 0;  // <- ¿es la PRIMERA compañía del documento?

            var company = new Company { Name = name };

            // UI
            Companies.Add(company);

            // Raíz que se serializa
            App.Companies.Add(company);

            // 🔑 Si es un documento NUEVO (primera compañía), asegúrate de que Guardar pida ruta
            if (creatingFirst)
                ProjectStorageService.Bind(App, path: null);

            SelectedCompany = company;
            ExportCommand?.RaiseCanExecuteChanged();
        }



        private void AddProject()
        {
            if (SelectedCompany == null) return;

            var name = AskText("Nuevo proyecto", "Nombre del proyecto:",
                               $"Proyecto {SelectedCompany.Projects.Count + 1}");
            if (name == null) return;

            var project = new Project { Name = name, StartDate = DateTime.Today };

            // (Opcional) mantener expandido lo que el usuario está viendo
            SelectedCompany.IsExpanded = true;
            project.IsExpanded = true;

            SelectedCompany.Projects.Add(project);
            SelectedProject = project;
        }

        private void AddWeek()
        {
            if (SelectedProject == null) return;

            var next = (SelectedProject.Weeks.Any()
                        ? SelectedProject.Weeks.Max(w => w.WeekNumber)
                        : 0) + 1;

            var weekStr = AskText("Nueva semana", "Número de semana:", next.ToString());
            if (weekStr == null) return;

            if (!int.TryParse(weekStr, out int weekNum) || weekNum <= 0)
            {
                MessageBox.Show("Número de semana inválido.", "Aviso",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedProject.Weeks.Any(w => w.WeekNumber == weekNum))
            {
                MessageBox.Show($"La Semana {weekNum} ya existe en {SelectedProject.Name}.",
                                "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 🔹 Auto-expandir para que se vea inmediatamente
            SelectedCompany!.IsExpanded = true;
            SelectedProject.IsExpanded = true;

            // Crear la semana
            var week = new WeekData
            {
                WeekNumber = weekNum,
                Notes = $"Semana {weekNum} creada"
            };

            // === PRELLENADO desde la semana previa más reciente ===
            CarryOverService.ApplyFromPrevious(SelectedProject!, week);

            // Agregar al proyecto y seleccionar
            SelectedProject.Weeks.Add(week);
            SelectedWeek = week;
        }


        private void RenameCompany()
        {
            if (SelectedCompany == null) return;

            var name = AskText("Renombrar compañía", "Nuevo nombre:", SelectedCompany.Name);
            if (name == null) return;

            SelectedCompany.Name = name;               // INotifyPropertyChanged en Company
            OnPropertyChanged(nameof(SelectedCompany)); // ya lo tienes y está bien
        }

        private void RenameProject()
        {
            if (SelectedProject == null) return;

            var name = AskText("Renombrar proyecto", "Nuevo nombre:", SelectedProject.Name);
            if (name == null) return;

            SelectedProject.Name = name;               // INotifyPropertyChanged en Project
            OnPropertyChanged(nameof(SelectedProject));
        }


        private void RenameWeek()
        {
            if (SelectedProject == null || SelectedWeek == null) return;

            var current = SelectedWeek.WeekNumber.ToString();
            var txt = AskText("Renombrar semana", "Número de semana:", current);
            if (txt == null) return;

            if (!int.TryParse(txt, out int n) || n <= 0)
            {
                MessageBox.Show("Número inválido.", "Aviso",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedProject.Weeks.Any(w => w != SelectedWeek && w.WeekNumber == n))
            {
                MessageBox.Show($"Ya existe la Semana {n}.", "Aviso",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedWeek.WeekNumber = n;               // INotifyPropertyChanged en WeekData
            OnPropertyChanged(nameof(SelectedWeek));
        }


        private void DeleteCompany()
        {
            if (SelectedCompany == null) return;

            var res = MessageBox.Show(
                $"¿Seguro que quiere cerrar '{SelectedCompany.Name}'?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res != MessageBoxResult.Yes) return;

            // ❗ Quitar de la raíz del documento (lo que se serializa)
            App.Companies.Remove(SelectedCompany);

            // Quitar de la colección que ve el TreeView
            Companies.Remove(SelectedCompany);

            // Limpiar selección
            SelectedCompany = null;
            SelectedProject = null;
            SelectedWeek = null;

            ExportCommand?.RaiseCanExecuteChanged();
        }



        private void DeleteProject()
        {
            if (SelectedCompany == null || SelectedProject == null) return;

            var res = MessageBox.Show(
                $"¿Eliminar proyecto '{SelectedProject.Name}' y sus semanas?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res != MessageBoxResult.Yes) return;

            SelectedCompany.Projects.Remove(SelectedProject);

            // Limpia selección
            SelectedProject = null;
            SelectedWeek = null;

            // (Opcional) si quieres re-evaluar Exportar aquí
            // ExportCommand?.RaiseCanExecuteChanged();
        }

        private void DeleteWeek()
        {
            if (SelectedProject == null || SelectedWeek == null) return;

            var res = MessageBox.Show(
                $"¿Eliminar {SelectedWeek}?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res != MessageBoxResult.Yes) return;

            SelectedProject.Weeks.Remove(SelectedWeek);

            // Limpia selección
            SelectedWeek = null;

            // (Opcional) si quieres re-evaluar Exportar aquí
            // ExportCommand?.RaiseCanExecuteChanged();
        }

        private void OpenPersonal()
        {
            if (SelectedCompany == null || SelectedProject == null || SelectedWeek == null)
                return;

            var shell = new ShellWindow(SelectedCompany, SelectedProject, SelectedWeek, this)
            {
                Owner = Application.Current.MainWindow
            };

            shell.Show(); // puedes usar ShowDialog() si quieres que bloquee el Main
        }





        // ====== Exportar ======
        private async Task ExportAsync()
        {
            // Sugerir nombre (si hay compañía seleccionada, úsala)
            string Suggest()
            {
                string San(string? s)
                {
                    if (string.IsNullOrWhiteSpace(s)) return "SinNombre";
                    foreach (var ch in System.IO.Path.GetInvalidFileNameChars()) s = s.Replace(ch, '_');
                    return s.Trim();
                }
                var companyName = SelectedCompany?.Name;
                var today = DateTime.Now.ToString("yyyy-MM-dd");
                return companyName != null
                    ? $"{San(companyName)} - {today}.kpi.json"
                    : $"ConcentradoKPI - {today}.kpi.json";
            }

            var sfd = new SaveFileDialog
            {
                FileName = Suggest(),
                Filter = "Concentrado KPI (*.kpi.json)|*.kpi.json|JSON (*.json)|*.json",
                Title = "Guardar como"
            };

            if (sfd.ShowDialog() == true)
            {
                // 👇 Guardamos la MISMA instancia que editas (App) y la dejamos anclada a esa ruta
                await ProjectStorageService.SaveToAsync(sfd.FileName, App, bind: true);

                Mensaje = $"Guardado: {sfd.FileName}";
                OnPropertyChanged(nameof(Mensaje));
            }
        }



        private AppData BuildExportData(ExportScope scope, Company? company, Project? project)
        {
            var data = new AppData();
            switch (scope)
            {
                case ExportScope.All:
                    foreach (var c in Companies) data.Companies.Add(c);
                    break;

                case ExportScope.Company:
                    if (company != null) data.Companies.Add(company);
                    break;

                case ExportScope.Project:
                    if (company != null && project != null)
                    {
                        var c = new Company { Name = company.Name, Description = company.Description };
                        var p = new Project
                        {
                            Name = project.Name,
                            Code = project.Code,
                            Location = project.Location,
                            Owner = project.Owner,
                            StartDate = project.StartDate,
                            EndDate = project.EndDate
                        };
                        foreach (var w in project.Weeks)
                        {
                            p.Weeks.Add(new WeekData
                            {
                                WeekNumber = w.WeekNumber,
                                WeekStart = w.WeekStart,
                                WeekEnd = w.WeekEnd,
                                Kpis = w.Kpis?.Select(k => new KpiValue { Name = k.Name, Value = k.Value, Unit = k.Unit }).ToList() ?? new(),

                                // ⬇️ si w.Pyramid fuera null, evita crash y crea una pirámide vacía
                                Pyramid = w.Pyramid != null ? new SafetyPyramid
                                {
                                    UnsafeActs = w.Pyramid.UnsafeActs,
                                    UnsafeConditions = w.Pyramid.UnsafeConditions,
                                    NearMisses = w.Pyramid.NearMisses,
                                    FirstAids = w.Pyramid.FirstAids,
                                    MedicalTreatments = w.Pyramid.MedicalTreatments,
                                    LostTimeInjuries = w.Pyramid.LostTimeInjuries,
                                    Fatalities = w.Pyramid.Fatalities,
                                    FindingsCorrected = w.Pyramid.FindingsCorrected,
                                    FindingsTotal = w.Pyramid.FindingsTotal,
                                    TrainingsCompleted = w.Pyramid.TrainingsCompleted,
                                    TrainingsPlanned = w.Pyramid.TrainingsPlanned,
                                    AuditsCompleted = w.Pyramid.AuditsCompleted,
                                    AuditsPlanned = w.Pyramid.AuditsPlanned
                                } : new SafetyPyramid(),

                                // ⬇️ si w.Tables fuera null, deja lista vacía
                                Tables = w.Tables?.Select(t => new TableData
                                {
                                    Name = t.Name,
                                    Columns = t.Columns?.ToList() ?? new(),
                                    Personas = t.Personas?.Select(r => r.ToList()).ToList() ?? new()
                                }).ToList() ?? new(),

                                Notes = w.Notes
                            });
                        }

                        c.Projects.Add(p);
                        data.Companies.Add(c);
                    }
                    break;
            }
            return data;
        }
        // === Helpers de reemplazo a nivel compañía/proyecto ===

        private Company? AskCompany(IList<Company> companies, Company? preselect)
        {
            // abre el mini-diálogo para elegir compañía
            var owner = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
            var dlg = new Views.SelectCompanyDialog(companies, preselect) { Owner = owner };
            return dlg.ShowDialog() == true ? dlg.SelectedCompany : null;
        }

        private void ReplaceCompany(Company existing, Company incoming)
        {
            // Propiedades simples
            existing.Name = incoming.Name;
            existing.Description = incoming.Description;

            // Reemplazo limpio de proyectos
            existing.Projects.Clear();
            foreach (var p in incoming.Projects)
                existing.Projects.Add(p);

            // (Opcional) auto-expandir
            existing.IsExpanded = true;
            foreach (var p in existing.Projects) p.IsExpanded = true;
        }

        private void ReplaceProject(Company parent, Project existingProject, Project incomingProject)
        {
            // Sacamos el proyecto existente y metemos el nuevo
            var index = parent.Projects.IndexOf(existingProject);
            if (index >= 0) parent.Projects.RemoveAt(index);

            // (Opcional) auto-expandir el nuevo
            incomingProject.IsExpanded = true;

            // Insertar en la misma posición si se puede, si no, al final
            if (index >= 0 && index <= parent.Projects.Count)
                parent.Projects.Insert(index, incomingProject);
            else
                parent.Projects.Add(incomingProject);
        }

        private string GetSuggestedFileName(ExportScope scope, Company? company, Project? project)
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd"); // seguro (sin '/')
            string San(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return "SinNombre";
                foreach (var ch in System.IO.Path.GetInvalidFileNameChars())
                    s = s.Replace(ch, '_');
                return s.Trim();
            }

            return scope switch
            {
                ExportScope.Project when company != null && project != null =>
                    $"{San(company.Name)} - {San(project.Name)} - {today}.kpi.json",
                ExportScope.Company when company != null =>
                    $"{San(company.Name)} - {today}.kpi.json",
                _ => $"ConcentradoKPI - {today}.kpi.json"
            };
        }

        private Company? FindCompanyForProject(Project proj)
            => Companies.FirstOrDefault(c => c.Projects.Contains(proj));

        // ====== Importar ======
        private async Task ImportAsync()
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Concentrado KPI (*.kpi.json)|*.kpi.json|JSON (*.json)|*.json",
                Title = "Importar"
            };
            if (ofd.ShowDialog() != true) return;

            var imported = await ProjectStorageService.ImportAsync(ofd.FileName, bind: true);
            if (imported is null || imported.Companies.Count == 0)
            {
                MessageBox.Show("Archivo vacío o inválido.", "Importar",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ====== TU BLOQUE DE MEZCLA/RESOLUCIÓN DE CONFLICTOS ======
            foreach (var incCompany in imported.Companies)
            {
                var existingCompany = Companies.FirstOrDefault(x => x.Name == incCompany.Name);

                if (existingCompany == null)
                {
                    Companies.Add(incCompany);
                    continue;
                }

                var ansCompany = MessageBox.Show(
                    $"La compañía '{incCompany.Name}' ya existe.\n\n" +
                    "Sí = REEMPLAZAR toda la compañía\n" +
                    "No = Mantener compañía y resolver por PROYECTO\n" +
                    "Cancelar = Abortar importación",
                    "Importar - Conflicto de compañía",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (ansCompany == MessageBoxResult.Cancel) return;

                if (ansCompany == MessageBoxResult.Yes)
                {
                    ReplaceCompany(existingCompany, incCompany);
                    continue;
                }

                foreach (var incProject in incCompany.Projects)
                {
                    var existingProject = existingCompany.Projects.FirstOrDefault(p => p.Name == incProject.Name);
                    if (existingProject == null)
                    {
                        existingCompany.Projects.Add(incProject);
                        continue;
                    }

                    var ansProject = MessageBox.Show(
                        $"En compañía '{existingCompany.Name}', el proyecto '{incProject.Name}' ya existe.\n\n" +
                        "Sí = REEMPLAZAR todo el proyecto\n" +
                        "No = Mantener proyecto y resolver por SEMANA\n" +
                        "Cancelar = Abortar importación",
                        "Importar - Conflicto de proyecto",
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                    if (ansProject == MessageBoxResult.Cancel) return;

                    if (ansProject == MessageBoxResult.Yes)
                    {
                        ReplaceProject(existingCompany, existingProject, incProject);
                        continue;
                    }

                    foreach (var w in incProject.Weeks)
                    {
                        var exWeek = existingProject.Weeks.FirstOrDefault(x => x.WeekNumber == w.WeekNumber);
                        if (exWeek == null)
                        {
                            existingProject.Weeks.Add(w);
                        }
                        else
                        {
                            var ansWeek = MessageBox.Show(
                                $"La {w} ya existe en '{existingProject.Name}'.\n\n" +
                                "Sí = Reemplazar esta semana\n" +
                                "No = Mantener la existente\n" +
                                "Cancelar = Abortar importación",
                                "Importar - Conflicto de semana",
                                MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                            if (ansWeek == MessageBoxResult.Cancel) return;
                            if (ansWeek == MessageBoxResult.Yes)
                            {
                                exWeek.WeekStart = w.WeekStart;
                                exWeek.WeekEnd = w.WeekEnd;

                                exWeek.Kpis = w.Kpis?.Select(k => new KpiValue
                                {
                                    Name = k.Name,
                                    Value = k.Value,
                                    Unit = k.Unit
                                }).ToList() ?? new();

                                exWeek.Pyramid = w.Pyramid != null ? new SafetyPyramid
                                {
                                    UnsafeActs = w.Pyramid.UnsafeActs,
                                    UnsafeConditions = w.Pyramid.UnsafeConditions,
                                    NearMisses = w.Pyramid.NearMisses,
                                    FirstAids = w.Pyramid.FirstAids,
                                    MedicalTreatments = w.Pyramid.MedicalTreatments,
                                    LostTimeInjuries = w.Pyramid.LostTimeInjuries,
                                    Fatalities = w.Pyramid.Fatalities,
                                    FindingsCorrected = w.Pyramid.FindingsCorrected,
                                    FindingsTotal = w.Pyramid.FindingsTotal,
                                    TrainingsCompleted = w.Pyramid.TrainingsCompleted,
                                    TrainingsPlanned = w.Pyramid.TrainingsPlanned,
                                    AuditsCompleted = w.Pyramid.AuditsCompleted,
                                    AuditsPlanned = w.Pyramid.AuditsPlanned
                                } : new SafetyPyramid();

                                exWeek.Tables = w.Tables?.Select(t => new TableData
                                {
                                    Name = t.Name,
                                    Columns = t.Columns?.ToList() ?? new(),
                                    Personas = t.Personas?.Select(r => r.ToList()).ToList() ?? new()
                                }).ToList() ?? new();

                                exWeek.Notes = w.Notes;
                            }
                        }
                    }
                }
            }

            // ⬇️ Sincroniza App con lo que quedó en Companies (para que Guardar serialice ESTA instancia)
            App = ProjectStorageService.CurrentData ?? new AppData();  // por seguridad
            App.Companies.Clear();
            foreach (var c in Companies) App.Companies.Add(c);

            // (opcional) asegúrate de que el servicio siga apuntando a la misma instancia + ruta abierta
            ProjectStorageService.Bind(App, ProjectStorageService.CurrentPath);

            // ====== UI/estado ======
            foreach (var c in Companies)
            {
                c.IsExpanded = true;
                foreach (var p in c.Projects) p.IsExpanded = true;
            }

            ExportCommand?.RaiseCanExecuteChanged();

            SelectedCompany = Companies.FirstOrDefault();
            SelectedProject = SelectedCompany?.Projects.FirstOrDefault();
            SelectedWeek = SelectedProject?.Weeks.FirstOrDefault();

            Mensaje = $"Importación aplicada ({App.Companies.Count} compañías en archivo).";
            OnPropertyChanged(nameof(Mensaje));
        }


        // ====== INotifyPropertyChanged ======
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));



        private string? AskText(string title, string prompt, string? initial = null)
        {
            // Busca ventana activa (MainWindow)
            var owner = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
            var dlg = new Views.InputDialog(title, prompt, initial)
            {
                Owner = owner
            };
            return dlg.ShowDialog() == true ? dlg.ResultText : null;
        }

        // ==== Helpers de CARRY OVER (PÉGALOS AQUÍ, DENTRO DE LA CLASE) ====

        /// Devuelve la semana existente más reciente con WeekNumber menor a target.
        private WeekData? GetMostRecentPreviousWeek(Project project, int newWeekNumber)
            => project.Weeks
                      .Where(w => w.WeekNumber < newWeekNumber)
                      .OrderByDescending(w => w.WeekNumber)
                      .FirstOrDefault();

        /// Aplica el prellenado (empresa ya está en Company; aquí tratamos Personal Vigente y Pirámide).
        private void ApplyCarryOverFromPrevious(Project project, WeekData targetWeek)
        {
            var prev = GetMostRecentPreviousWeek(project, targetWeek.WeekNumber);
            if (prev == null) return;

            // === Diagnóstico simple (para saber de dónde vamos a clonar) ===
            try
            {
                bool hasPvd = prev.GetType().GetProperty("PersonalVigenteDocument") != null;
                bool hasPirS = prev.GetType().GetProperty("PiramideSeguridad") != null;

                MessageBox.Show(
                    "Prev: " + prev.WeekNumber + "\n" +
                    "Tables: " + (prev.Tables != null ? prev.Tables.Count : 0) + "\n" +
                    "Pyramid (SafetyPyramid): " + (prev.Pyramid != null ? "si" : "no") + "\n" +
                    "Tiene PersonalVigenteDocument: " + (hasPvd ? "si" : "no") + "\n" +
                    "Tiene PiramideSeguridad: " + (hasPirS ? "si" : "no"),
                    "CarryOver - Inspect",
                    MessageBoxButton.OK, MessageBoxImage.Information
                );
            }
            catch { /* no bloquear */ }

            bool clonedSomething = false;

            // ====== OPCIÓN A: Esquema por Tables + SafetyPyramid ======

            // 1) Personal Vigente por tablas (clonar filas, resetear horas a 0)
            if (prev.Tables != null && prev.Tables.Count > 0)
            {
                targetWeek.Tables ??= new List<TableData>();
                targetWeek.Tables.Clear();

                foreach (var t in prev.Tables)
                {
                    // Si quisieras solo una tabla específica:
                    // if (!string.Equals(t.Name, "Personal Vigente", StringComparison.OrdinalIgnoreCase)) continue;

                    targetWeek.Tables.Add(CloneTableResetHours(t));
                    clonedSomething = true;
                }
            }

            // 2) Pirámide (SafetyPyramid)
            if (prev.Pyramid != null)
            {
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

            // 3) KPIs: si alguno es de horas → 0
            if (prev.Kpis != null && prev.Kpis.Count > 0)
            {
                targetWeek.Kpis = prev.Kpis
                    .Select(k => new KpiValue
                    {
                        Name = k.Name,
                        Unit = k.Unit,
                        Value = IsHoursField(k.Name) ? 0 : k.Value
                    })
                    .ToList();
                clonedSomething = true;
            }

            // ====== OPCIÓN B (fallback): propiedades alternas por reflexión ======
            // Solo entra si no clonamos nada con Tables/Pyramid/Kpis.
            if (!clonedSomething)
            {
                try
                {
                    // PersonalVigenteDocument (si existe en WeekData)
                    var pvdProp = prev.GetType().GetProperty("PersonalVigenteDocument");
                    if (pvdProp != null)
                    {
                        var prevPvd = pvdProp.GetValue(prev);
                        if (prevPvd != null)
                        {
                            var targetPvdProp = typeof(WeekData).GetProperty("PersonalVigenteDocument");
                            if (targetPvdProp != null)
                            {
                                var pvdType = pvdProp.PropertyType;
                                var newPvd = Activator.CreateInstance(pvdType);

                                // Campos típicos de empresa
                                string[] empresaProps = { "EmpresaNombre", "RazonSocial", "RFC", "DomicilioFiscal", "Representante", "TelefonoContacto", "CorreoContacto" };
                                foreach (var nm in empresaProps)
                                {
                                    var src = pvdType.GetProperty(nm);
                                    var dst = pvdType.GetProperty(nm);
                                    if (src != null && dst != null) dst.SetValue(newPvd, src.GetValue(prevPvd));
                                }

                                // Listado de personal con horas = 0 si existe
                                var personalProp = pvdType.GetProperty("PersonalVigente");
                                if (personalProp != null)
                                {
                                    var prevList = personalProp.GetValue(prevPvd) as System.Collections.IEnumerable;
                                    if (prevList != null)
                                    {
                                        var listType = personalProp.PropertyType;
                                        var newList = Activator.CreateInstance(listType) as System.Collections.IList;

                                        foreach (var per in prevList)
                                        {
                                            var perType = per.GetType();
                                            var newPer = Activator.CreateInstance(perType);

                                            string[] fields = { "Id", "NombreCompleto", "Puesto", "Area", "NoEmpleado", "TipoContrato", "Turno" };
                                            foreach (var f in fields)
                                            {
                                                var pp = perType.GetProperty(f);
                                                if (pp != null)
                                                    perType.GetProperty(f)?.SetValue(newPer, pp.GetValue(per));
                                            }

                                            var horasProp = perType.GetProperty("HorasTrabajadas");
                                            if (horasProp != null)
                                            {
                                                if (horasProp.PropertyType == typeof(int) || horasProp.PropertyType == typeof(int?))
                                                    horasProp.SetValue(newPer, 0);
                                                else if (horasProp.PropertyType == typeof(double) || horasProp.PropertyType == typeof(double?))
                                                    horasProp.SetValue(newPer, 0.0);
                                                else if (horasProp.PropertyType == typeof(string))
                                                    horasProp.SetValue(newPer, "0");
                                            }

                                            newList?.Add(newPer);
                                        }

                                        personalProp.SetValue(newPvd, newList);
                                    }
                                }

                                // Horas totales semanales = 0 si existe
                                var horasTotProp = pvdType.GetProperty("HorasTrabajadasTotal");
                                if (horasTotProp != null)
                                {
                                    if (horasTotProp.PropertyType == typeof(int) || horasTotProp.PropertyType == typeof(int?))
                                        horasTotProp.SetValue(newPvd, 0);
                                    else if (horasTotProp.PropertyType == typeof(double) || horasTotProp.PropertyType == typeof(double?))
                                        horasTotProp.SetValue(newPvd, 0.0);
                                    else if (horasTotProp.PropertyType == typeof(string))
                                        horasTotProp.SetValue(newPvd, "0");
                                }

                                targetPvdProp.SetValue(targetWeek, newPvd);
                                clonedSomething = true;
                            }
                        }
                    }

                    // PiramideSeguridad (si existiera con ese nombre)
                    var pirProp = prev.GetType().GetProperty("PiramideSeguridad");
                    if (pirProp != null)
                    {
                        var prevPir = pirProp.GetValue(prev);
                        if (prevPir != null)
                        {
                            var targetPirProp = typeof(WeekData).GetProperty("PiramideSeguridad");
                            if (targetPirProp != null)
                            {
                                var pirType = pirProp.PropertyType;
                                var newPir = Activator.CreateInstance(pirType);

                                string[] simples = { "FAI", "MTI", "MDI", "LTI" };
                                foreach (var nm in simples)
                                {
                                    var sp = pirType.GetProperty(nm);
                                    var tp = pirType.GetProperty(nm);
                                    if (sp != null && tp != null) tp.SetValue(newPir, sp.GetValue(prevPir));
                                }

                                // Copiar listas si existen (Precursores/Potenciales)
                                void CopyList(string name)
                                {
                                    var sp = pirType.GetProperty(name);
                                    if (sp == null) return;
                                    var list = sp.GetValue(prevPir) as System.Collections.IEnumerable;
                                    if (list == null) return;

                                    var listType = sp.PropertyType;
                                    var newList = Activator.CreateInstance(listType) as System.Collections.IList;

                                    foreach (var item in list)
                                    {
                                        var it = item?.GetType();
                                        var newItem = it != null ? Activator.CreateInstance(it) : null;

                                        if (newItem != null && it != null)
                                        {
                                            foreach (var pr in it.GetProperties())
                                            {
                                                // Si alguna subpropiedad fuese horas, la ponemos en 0
                                                if (pr.Name.ToLowerInvariant().Contains("hora"))
                                                {
                                                    if (pr.PropertyType == typeof(int) || pr.PropertyType == typeof(int?)) pr.SetValue(newItem, 0);
                                                    else if (pr.PropertyType == typeof(double) || pr.PropertyType == typeof(double?)) pr.SetValue(newItem, 0.0);
                                                    else if (pr.PropertyType == typeof(string)) pr.SetValue(newItem, "0");
                                                }
                                                else
                                                {
                                                    pr.SetValue(newItem, pr.GetValue(item));
                                                }
                                            }
                                            newList?.Add(newItem);
                                        }
                                    }

                                    sp.SetValue(newPir, newList);
                                }

                                CopyList("Precursores");
                                CopyList("Potenciales");

                                var horasTotProp2 = pirType.GetProperty("HorasTrabajadasTotal");
                                if (horasTotProp2 != null)
                                {
                                    if (horasTotProp2.PropertyType == typeof(int) || horasTotProp2.PropertyType == typeof(int?))
                                        horasTotProp2.SetValue(newPir, 0);
                                    else if (horasTotProp2.PropertyType == typeof(double) || horasTotProp2.PropertyType == typeof(double?))
                                        horasTotProp2.SetValue(newPir, 0.0);
                                    else if (horasTotProp2.PropertyType == typeof(string))
                                        horasTotProp2.SetValue(newPir, "0");
                                }

                                targetPirProp.SetValue(targetWeek, newPir);
                                clonedSomething = true;
                            }
                        }
                    }
                }
                catch { /* no bloquear */ }
            }

            // Nota
            targetWeek.Notes = $"Semana {targetWeek.WeekNumber} creada a partir de semana {prev.WeekNumber}";
        }


        /// Clona tabla copiando columnas/filas y reseteando columnas de horas a "0".
        private TableData CloneTableResetHours(TableData src)
        {
            var clone = new TableData
            {
                Name = src.Name,
                Columns = src.Columns?.ToList() ?? new List<string>(),
                Personas = new List<List<string>>()
            };

            if (src.Personas != null && src.Columns != null)
            {
                var hourColIdx = src.Columns
                    .Select((col, idx) => (col, idx))
                    .Where(t => IsHoursField(t.col))
                    .Select(t => t.idx)
                    .ToHashSet();

                foreach (var row in src.Personas)
                {
                    var newRow = row.ToList();
                    foreach (var idx in hourColIdx)
                    {
                        if (idx >= 0 && idx < newRow.Count)
                            newRow[idx] = "0";
                    }
                    clone.Personas.Add(newRow);
                }
            }

            return clone;
        }

        /// Heurística para detectar columnas/campos que son "horas trabajadas".
        private bool IsHoursField(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            var n = name.Trim().ToLowerInvariant();
            string[] keys =
            {
            "hora", "horas", "hrs", "h/trab", "horas trabajadas", "h. trabajadas",
            "he", "horas hombre", "hh", "hs", "tiempo trabajado"
        };
            return keys.Any(k => n.Contains(k));
        }

    } 

} 