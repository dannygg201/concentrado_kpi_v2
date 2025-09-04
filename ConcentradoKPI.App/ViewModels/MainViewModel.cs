using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO; 
using System.Linq;
using System.Threading.Tasks;
using System.Windows;                 // MessageBox
using Microsoft.Win32;                // OpenFileDialog/SaveFileDialog
using ConcentradoKPI.App.Models;
using ConcentradoKPI.App.Services;
using System.Windows.Data;
using System;
using System.Collections.Generic;

namespace ConcentradoKPI.App.ViewModels
{
    public enum ExportScope { All, Company, Project, OneWeek, ManyWeeks }

    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Company> Companies { get; } = new();

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

            }
        }

        public string Mensaje { get; set; } = "Selecciona o crea compañías, proyectos y semanas.";
        private readonly ProjectStorageService _storage = new();

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

            OpenPersonalCommand = new RelayCommand(
                _ =>
                {
                    if (SelectedCompany != null && SelectedProject != null && SelectedWeek != null)
                        OpenPersonalRequested?.Invoke(SelectedCompany, SelectedProject, SelectedWeek);
},
    _ => SelectedWeek != null
);


            RefreshCommandStates();
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
            var company = new Company { Name = name };
            Companies.Add(company);
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

            var week = new WeekData { WeekNumber = weekNum, Notes = $"Semana {weekNum} creada" };
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
                $"¿Eliminar compañía '{SelectedCompany.Name}' y TODO su contenido?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res != MessageBoxResult.Yes) return;

            Companies.Remove(SelectedCompany);

            // Limpia selección
            SelectedCompany = null;
            SelectedProject = null;
            SelectedWeek = null;

            // Puede cambiar el estado de Exportar
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
            if (SelectedCompany == null || SelectedProject == null || SelectedWeek == null) return;
            OpenPersonalRequested?.Invoke(SelectedCompany, SelectedProject, SelectedWeek);
        }


        // ====== Exportar ======
        private async Task ExportAsync()
        {
            // 1) Elegir compañía a exportar
            var company = SelectedCompany;

            // si no hay seleccionada o quieres forzar selección cuando haya más de una, pide al usuario
            if (Companies.Count == 0)
            {
                MessageBox.Show("No hay compañías para guardar.", "Guardar como",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (company == null || Companies.Count > 1)
            {
                company = AskCompany(Companies, SelectedCompany);
                if (company == null) return; // canceló
            }

            // 2) Construir AppData SOLO con esa compañía
            var data = new AppData();
            data.Companies.Add(company);

            // 3) Sugerir nombre y preguntar sobrescritura si existe
            var sfd = new SaveFileDialog
            {
                FileName = $"{San(company.Name)} - {DateTime.Now:yyyy-MM-dd}.kpi.json",
                Filter = "Concentrado KPI (*.kpi.json)|*.kpi.json|JSON (*.json)|*.json",
                Title = "Guardar como"
            };

            if (sfd.ShowDialog() == true)
            {
                if (System.IO.File.Exists(sfd.FileName))
                {
                    var overwrite = MessageBox.Show(
                        "El archivo ya existe. ¿Deseas reemplazarlo?",
                        "Guardar como",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (overwrite != MessageBoxResult.Yes) return;
                }

                await _storage.ExportAsync(sfd.FileName, data);
                Mensaje = $"Guardado: {sfd.FileName}";
                OnPropertyChanged(nameof(Mensaje));
            }

            string San(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return "SinNombre";
                foreach (var ch in System.IO.Path.GetInvalidFileNameChars()) s = s.Replace(ch, '_');
                return s.Trim();
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
                                    Rows = t.Rows?.Select(r => r.ToList()).ToList() ?? new()
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

            var imported = await _storage.ImportAsync(ofd.FileName);
            if (imported is null || imported.Companies.Count == 0)
            {
                MessageBox.Show("Archivo vacío o inválido.", "Importar",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- Sin "Reemplazar todo". Vamos compañía por compañía.
            foreach (var incCompany in imported.Companies)
            {
                var existingCompany = Companies.FirstOrDefault(x => x.Name == incCompany.Name);

                if (existingCompany == null)
                {
                    // Nueva compañía → se agrega tal cual
                    Companies.Add(incCompany);
                    continue;
                }

                // Ya existe esa compañía → preguntar qué hacer
                var ansCompany = MessageBox.Show(
                    $"La compañía '{incCompany.Name}' ya existe.\n\n" +
                    "Sí = REEMPLAZAR toda la compañía\n" +
                    "No = Mantener compañía y resolver por PROYECTO\n" +
                    "Cancelar = AbortAR importación",
                    "Importar - Conflicto de compañía",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (ansCompany == MessageBoxResult.Cancel) return;

                if (ansCompany == MessageBoxResult.Yes)
                {
                    // Reemplazar compañía completa
                    ReplaceCompany(existingCompany, incCompany);
                    continue;
                }

                // No → Resolver por PROYECTO
                foreach (var incProject in incCompany.Projects)
                {
                    var existingProject = existingCompany.Projects
                                                         .FirstOrDefault(p => p.Name == incProject.Name);
                    if (existingProject == null)
                    {
                        existingCompany.Projects.Add(incProject);
                        continue;
                    }

                    // Ya existe ese proyecto → preguntar
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
                        // Reemplazar proyecto completo
                        ReplaceProject(existingCompany, existingProject, incProject);
                        continue;
                    }

                    // No → Resolver por SEMANA (tu lógica existente por semana)
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
                                // Reemplazar contenido de esa semana (a prueba de null)
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
                                    Rows = t.Rows?.Select(r => r.ToList()).ToList() ?? new()
                                }).ToList() ?? new();

                                exWeek.Notes = w.Notes;
                            }
                        }
                    }
                }
            }

            // Auto-expandir todo lo que quedó después de importar
            foreach (var c in Companies)
            {
                c.IsExpanded = true;
                foreach (var p in c.Projects) p.IsExpanded = true;
            }

            ExportCommand?.RaiseCanExecuteChanged();

            // Selección amigable (opcional)
            SelectedCompany = Companies.FirstOrDefault();
            SelectedProject = SelectedCompany?.Projects.FirstOrDefault();
            SelectedWeek = SelectedProject?.Weeks.FirstOrDefault();

            Mensaje = $"Importación aplicada ({imported.Companies.Count} compañías en archivo).";
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

    } }
