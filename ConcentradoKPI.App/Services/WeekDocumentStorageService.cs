using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace ConcentradoKPI.App.Services
{
    public static class WeekDocumentStorageService
    {
        public static string? CurrentPath { get; private set; }
        public static object? CurrentDoc { get; private set; }

        private static readonly DirtyTracker<object> _dirty =
            new DirtyTracker<object>(new JsonSerializerOptions
            {
                WriteIndented = false,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            });

        public static bool IsDirty => CurrentDoc != null && _dirty.IsDirty(CurrentDoc);

        public static void Bind(object doc, string? path)
        {
            CurrentDoc = doc;
            CurrentPath = path;
            _dirty.MarkSaved(doc);
        }

        private static readonly JsonSerializerOptions _json = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        public static async Task<bool> SaveOrPromptAsync(Window owner, object doc)
        {
            if (string.IsNullOrWhiteSpace(CurrentPath))
            {
                var dlg = BuildSaveDialog(CurrentPath);
                if (dlg.ShowDialog(owner) != true) return false;
                CurrentPath = dlg.FileName;
            }
            await SaveToAsync(CurrentPath!, doc);
            Bind(doc, CurrentPath);
            return true;
        }

        public static async Task<string?> SaveAsAsync(Window owner, object doc)
        {
            var dlg = BuildSaveDialog(CurrentPath);
            if (dlg.ShowDialog(owner) == true)
            {
                await SaveToAsync(dlg.FileName, doc);
                Bind(doc, dlg.FileName);
                return CurrentPath;
            }
            return null;
        }

        public static async Task SaveToAsync(string filePath, object doc)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            await using var fs = File.Create(filePath);
            await System.Text.Json.JsonSerializer.SerializeAsync(fs, doc, _json);
        }

        private static SaveFileDialog BuildSaveDialog(string? currentPath) => new SaveFileDialog
        {
            Title = "Guardar documento semanal",
            Filter = "Archivo KPI (*.kpi.json)|*.kpi.json|JSON (*.json)|*.json|Todos (*.*)|*.*",
            FileName = string.IsNullOrWhiteSpace(currentPath) ? "Semana.kpi.json" : Path.GetFileName(currentPath),
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
