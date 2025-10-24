// Services/ProjectStorageService.cs
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
        // ====== Estado del archivo actual ======
        public static AppData? CurrentData { get; private set; }
        public static string? CurrentPath { get; private set; }
        public static bool HasOpenFile => CurrentData != null && !string.IsNullOrWhiteSpace(CurrentPath);

        public static void Bind(AppData data, string? path)
        {
            CurrentData = data;
            CurrentPath = path;
        }

        // ====== JSON ======
        private static readonly JsonSerializerOptions _json = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        // ====== Abrir (Import) ======
        public static async Task<AppData?> ImportAsync(string filePath, bool bind = true)
        {
            if (!File.Exists(filePath)) return null;
            await using var fs = File.OpenRead(filePath);
            var data = await JsonSerializer.DeserializeAsync<AppData>(fs, _json);
            if (bind && data != null) Bind(data, filePath);
            return data;
        }

        // ====== Guardar (tipo Word): si hay ruta, guarda; si no, pide “Guardar como” ======
        public static async Task<string> SaveOrPromptAsync(Window owner)
        {
            if (CurrentData is null)
                throw new InvalidOperationException("No hay datos cargados para guardar.");

            if (string.IsNullOrWhiteSpace(CurrentPath))
            {
                var dlg = new SaveFileDialog
                {
                    Title = "Guardar archivo KPI",
                    Filter = "Archivo KPI (*.kpi.json)|*.kpi.json|JSON (*.json)|*.json|Todos (*.*)|*.*",
                    FileName = "Proyecto.kpi.json",
                    AddExtension = true,
                    OverwritePrompt = true
                };
                var ok = dlg.ShowDialog(owner);
                if (ok != true) throw new OperationCanceledException("Guardado cancelado.");
                CurrentPath = dlg.FileName;
            }

            await SaveToAsync(CurrentPath!, CurrentData, bind: true);
            return CurrentPath!;
        }

        // ====== Guardar como… ======
        public static async Task<string?> SaveAsAsync(Window owner, AppData data)
        {
            var dlg = new SaveFileDialog
            {
                Title = "Guardar como",
                Filter = "Archivo KPI (*.kpi.json)|*.kpi.json|JSON (*.json)|*.json|Todos (*.*)|*.*",
                FileName = "Proyecto.kpi.json",
                AddExtension = true,
                OverwritePrompt = true
            };
            var ok = dlg.ShowDialog(owner);
            if (ok == true)
            {
                await SaveToAsync(dlg.FileName, data, bind: true);
                return dlg.FileName;
            }
            return null;
        }

        // ====== Guardado directo a ruta (única definición) ======
        public static async Task SaveToAsync(string filePath, AppData data, bool bind = true)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            await using var fs = File.Create(filePath);
            await JsonSerializer.SerializeAsync(fs, data, _json);
            if (bind) Bind(data, filePath);
        }
    }
}
