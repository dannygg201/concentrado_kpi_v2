using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using ConcentradoKPI.App.Models;

namespace ConcentradoKPI.App.Services
{
    public static class ProjectStorageService
    {
        // ===== Estado del archivo actual =====
        public static AppData? CurrentData { get; private set; }
        public static string? CurrentPath { get; private set; }
        public static bool HasOpenFile => CurrentData != null && !string.IsNullOrWhiteSpace(CurrentPath);

        // ===== Dirty tracking =====
        private static readonly DirtyTracker<AppData> _dirty =
            new DirtyTracker<AppData>(new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            });

        // Flag manual para marcar sucio aunque aún no hayamos reflejado cambios en CurrentData
        private static bool _manualDirty;

        public static bool IsDirty =>
            (CurrentData != null && _dirty.IsDirty(CurrentData)) || _manualDirty;

        // Permite a las VMs marcar “hay cambios”
        public static void MarkDirty() => _manualDirty = true;

        private static void ClearDirtySnapshot(AppData data)
        {
            _dirty.MarkSaved(data);
            _manualDirty = false;
        }

        public static void Bind(AppData data, string? path)
        {
            CurrentData = data;
            CurrentPath = path;
            ClearDirtySnapshot(data); // snapshot limpio al enlazar
        }

        // ===== JSON =====
        private static readonly JsonSerializerOptions _json = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        // ===== Abrir (Import) =====
        public static async Task<AppData?> ImportAsync(string filePath, bool bind = true)
        {
            if (!File.Exists(filePath)) return null;
            await using var fs = File.OpenRead(filePath);
            var data = await JsonSerializer.DeserializeAsync<AppData>(fs, _json);
            if (bind && data != null) Bind(data, filePath);
            return data;
        }

        // ===== Guardar (si hay ruta) o pedir ruta (si no la hay) =====
        public static async Task<bool> SaveOrPromptAsync(Window owner)
        {
            if (CurrentData is null)
                throw new InvalidOperationException("No hay datos cargados para guardar.");

            if (string.IsNullOrWhiteSpace(CurrentPath))
            {
                var dlg = BuildSaveDialog(CurrentPath);
                if (dlg.ShowDialog(owner) != true) return false;
                CurrentPath = dlg.FileName;
            }

            await SaveToAsync(CurrentPath!, CurrentData, bind: true);
            return true;
        }

        // ===== Guardar como… (siempre pide ruta) =====
        public static async Task<string?> SaveAsAsync(Window owner, AppData data)
        {
            var dlg = BuildSaveDialog(CurrentPath);
            if (dlg.ShowDialog(owner) == true)
            {
                await SaveToAsync(dlg.FileName, data, bind: true);
                return dlg.FileName;
            }
            return null;
        }

        // ===== Guardar directo a ruta =====
        public static async Task SaveToAsync(string filePath, AppData data, bool bind = true)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            await using var fs = File.Create(filePath);
            await JsonSerializer.SerializeAsync(fs, data, _json);

            if (bind)
            {
                // asegura snapshot limpio y ruta actual
                Bind(data, filePath);
            }
            else
            {
                ClearDirtySnapshot(data);
            }
        }

        private static SaveFileDialog BuildSaveDialog(string? currentPath) => new SaveFileDialog
        {
            Title = "Guardar archivo KPI",
            Filter = "Archivo KPI (*.kpi.json)|*.kpi.json|JSON (*.json)|*.json|Todos (*.*)|*.*",
            FileName = string.IsNullOrWhiteSpace(currentPath) ? "Proyecto.kpi.json" : Path.GetFileName(currentPath),
            DefaultExt = ".json",
            AddExtension = true,
            OverwritePrompt = true,
            InitialDirectory = GetInitialDir(currentPath)
        };

        private static string GetInitialDir(string? pathHint)
        {
            try
            {
                var dir = string.IsNullOrWhiteSpace(pathHint) ? null : Path.GetDirectoryName(pathHint);
                if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir)) return dir!;
            }
            catch { }
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
    }
}
