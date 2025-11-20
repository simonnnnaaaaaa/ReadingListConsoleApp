using System.Globalization;
using System.Text;
using ReadingList.Domain;

namespace ReadingList.Infrastructure.Exporting
{
    public sealed class CsvExportStrategy : IExportStrategy
    {
        public string Name => "csv";

        private readonly int _delayMs;

        public CsvExportStrategy(int delayMs = 800) => _delayMs = delayMs;

        public async Task ExportAsync(IEnumerable<Book> books, string path, CancellationToken ct = default)
        {
            await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
            await using var sw = new StreamWriter(fs, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            await sw.WriteLineAsync("Id,Title,Author,Year,Pages,Genre,Finished,Rating");
            ct.ThrowIfCancellationRequested();

            static string Escape(string? s)
            {
                s ??= string.Empty;
                var needsQuotes = s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
                return needsQuotes ? $"\"{s.Replace("\"", "\"\"")}\"" : s;
            }

            foreach (var b in books)
            {
                ct.ThrowIfCancellationRequested();

                if (_delayMs > 0)
                    await Task.Delay(_delayMs, ct);

                var line = string.Join(",",
                    b.Id.ToString(CultureInfo.InvariantCulture),
                    Escape(b.Title),
                    Escape(b.Author),
                    b.YearPublished.ToString(CultureInfo.InvariantCulture),
                    b.NumberOfPages.ToString(CultureInfo.InvariantCulture),
                    Escape(b.Genre),
                    b.IsFinished ? "yes" : "no",
                    b.Rating.ToString(CultureInfo.InvariantCulture));

                await sw.WriteLineAsync(line);
            }

            await sw.FlushAsync();
            await fs.FlushAsync(ct);
        }
    }
}
