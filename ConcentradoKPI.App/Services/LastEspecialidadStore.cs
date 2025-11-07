using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ConcentradoKPI.App.Services
{
    /// <summary>
    /// Guarda/recupera la última "Especialidad" usada por Company+Project.
    /// Archivo: %AppData%\ConcentradoKPI\last_especialidad.json
    /// </summary>
    public static class LastEspecialidadStore
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ConcentradoKPI", "last_especialidad.json");

        private class Model
        {
            public Dictionary<string, string> Map { get; set; } = new();
        }

        private static string MakeKey(string company, string project)
            => $"{company}|{project}".ToUpperInvariant();

        private static Model Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<Model>(json) ?? new Model();
                }
            }
            catch
            {
                // tolerante a errores
            }
            return new Model();
        }

        private static void Save(Model m)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                var json = JsonSerializer.Serialize(m, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch
            {
                // tolerante a errores
            }
        }

        public static string? Get(string company, string project)
        {
            var m = Load();
            return m.Map.TryGetValue(MakeKey(company, project), out var val) ? val : null;
        }

        public static void Set(string company, string project, string? especialidad)
        {
            var m = Load();
            m.Map[MakeKey(company, project)] = especialidad ?? string.Empty;
            Save(m);
        }
    }
}
