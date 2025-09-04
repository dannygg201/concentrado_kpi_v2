using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ConcentradoKPI.App.Models;

namespace ConcentradoKPI.App.Services
{
    public class ProjectStorageService
    {
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public async Task ExportAsync(string filePath, AppData data)
        {
            using var fs = File.Create(filePath);
            await JsonSerializer.SerializeAsync(fs, data, _jsonOptions);
        }

        public async Task<AppData?> ImportAsync(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            using var fs = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<AppData>(fs, _jsonOptions);
        }
    }
}
