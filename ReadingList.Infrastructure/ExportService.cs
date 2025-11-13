using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ReadingList.Domain;

namespace ReadingList.Infrastructure
{
    public sealed class ExportService
    {
        private readonly int _delayMs;

        public ExportService(int delayMs = 800)
        {
            if (delayMs < 0) throw new ArgumentOutOfRangeException(nameof(delayMs));
            _delayMs = delayMs;
        }

        public async Task ExportJsonAsync(IEnumerable<Book> books, string path, CancellationToken ct = default)
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

        public async Task ExportCsvAsync(IEnumerable<Book> books, string path, CancellationToken ct = default)
        {
            await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
            await using var sw = new StreamWriter(fs, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            await sw.WriteLineAsync("Id,Title,Author,Year,Pages,Genre,Finished,Rating");
            ct.ThrowIfCancellationRequested();

            static string Escape(string? s)
            {
                s ??= string.Empty;
                var needsQuotes = s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
                if (!needsQuotes)
                {
                    return s;
                }
                return $"\"{s.Replace("\"", "\"\"")}\"";
            }

            foreach (var book in books)
            {
                ct.ThrowIfCancellationRequested();

                if (_delayMs > 0)
                    await Task.Delay(_delayMs, ct);  

                var line = string.Join(",",
                    book.Id.ToString(CultureInfo.InvariantCulture),
                    Escape(book.Title),
                    Escape(book.Author),
                    book.YearPublished.ToString(CultureInfo.InvariantCulture),
                    book.NumberOfPages.ToString(CultureInfo.InvariantCulture),
                    Escape(book.Genre),
                    book.IsFinished ? "yes" : "no",
                    book.Rating.ToString(CultureInfo.InvariantCulture));

                await sw.WriteLineAsync(line);
            }

            await sw.FlushAsync();
            await fs.FlushAsync(ct);

        }
    }
}