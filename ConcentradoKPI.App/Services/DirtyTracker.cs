using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ConcentradoKPI.App.Services
{
    public sealed class DirtyTracker<T>
    {
        private readonly JsonSerializerOptions _opts;
        private string? _lastSavedHash;

        public DirtyTracker(JsonSerializerOptions? opts = null)
        {
            _opts = opts ?? new JsonSerializerOptions { WriteIndented = false };
        }

        public bool IsDirty(T current)
        {
            var now = ComputeHash(current);
            return _lastSavedHash == null || !string.Equals(_lastSavedHash, now, StringComparison.Ordinal);
        }

        public void MarkSaved(T current) => _lastSavedHash = ComputeHash(current);

        private string ComputeHash(T obj)
        {
            var json = JsonSerializer.Serialize(obj, _opts);
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
            return Convert.ToHexString(bytes);
        }
    }
}
