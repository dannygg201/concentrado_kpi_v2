using System;
using System.IO;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;
using ConcentradoKPI.App.Models;

namespace ConcentradoKPI.App.Services
{
    public static class WeekDocumentStorageService
    {
        private static readonly JsonSerializerOptions _json = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        private static string BaseFolder =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         "ConcentradoKPI", "Weeks");

        private static string Safe(string s) =>
            string.Join("_", s.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

        private static string FilePath(string company, string project, int week)
        {
            Directory.CreateDirectory(BaseFolder);
            return Path.Combine(BaseFolder, $"{Safe(company)}__{Safe(project)}__W{week}.kpi.json");
        }

        public static async Task SaveAsync(PersonalVigenteDocument doc)
        {
            var path = FilePath(doc.Company, doc.Project, doc.WeekNumber);
            await using var fs = File.Create(path);
            await JsonSerializer.SerializeAsync(fs, doc, _json);
        }

        public static async Task<PersonalVigenteDocument?> LoadAsync(string company, string project, int week)
        {
            var path = FilePath(company, project, week);
            if (!File.Exists(path)) return null;
            await using var fs = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<PersonalVigenteDocument>(fs, _json);
        }

        public static string GetPath(string company, string project, int week)
            => FilePath(company, project, week);
    }
}
