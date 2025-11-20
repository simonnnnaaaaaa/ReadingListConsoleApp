using System.Text.Json;
using ReadingList.Domain;

namespace ReadingList.Infrastructure.Exporting
{
    public sealed class JsonExportStrategy : IExportStrategy
    {
        public string Name => "json";
        private readonly int _delayMs;
        public JsonExportStrategy(int delayMs = 800) => _delayMs = delayMs;

        public async Task ExportAsync(IEnumerable<Book> books, string path, CancellationToken ct = default)
        {
            var list = books.ToList();

            if (_delayMs > 0)
            {
                await Task.Delay(_delayMs, ct);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true 
            };

            await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
            await JsonSerializer.SerializeAsync(fs, list, options, ct);
            await fs.FlushAsync(ct);
        }
    }
}
