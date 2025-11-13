using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadingList.Domain;
using System.Threading;

namespace ReadingList.Infrastructure
{
    public sealed class ImportService
    {
        private readonly int _delayMs;

        public ImportService(int delayMs = 800)
        {
            if (delayMs < 0) throw new ArgumentOutOfRangeException(nameof(delayMs));
            _delayMs = delayMs;
        }

        public async Task<ImportSummary> ImportFileAsync(string path, IRepository<Book, int> repository, TextWriter? log = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            }

            if (repository is null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            var summary = new ImportSummary();

            string[] lines = await File.ReadAllLinesAsync(path, ct).ConfigureAwait(false);

            if (lines.Length == 0)
            {
                log?.WriteLine($"[warn] File '{path}' is empty.");
                return summary;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                ct.ThrowIfCancellationRequested(); // periodic check

                // DOAR ÎN DEBUG: simulăm I/O lent
                if (_delayMs > 0)
                { 
                    await Task.Delay(_delayMs, ct);
                }

                var line = lines[i];
                
                if(string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var parsed = CsvParser.ParseLine(line);

                if(!parsed.IsSuccess)
                {
                    log?.WriteLine($"[warn] Line {i + 1} is malformed: {parsed.ErrorMessage}. Skipping.");
                    summary.Malformed++;
                    continue;
                }

                var book = parsed.Value!;

                bool added = repository.Add(book);

                if (added)
                {
                    summary.Imported++;
                }
                else
                {
                    summary.Duplicates++;
                    summary.SkippedIds.Add(book.Id);
                }
            }

            return summary;

        }


        public async Task<ImportSummary> ImportFilesAsync(IEnumerable<string> paths, IRepository<Book, int> repository, TextWriter?log = null, CancellationToken ct = default)
        {
            var logger = log ?? TextWriter.Null;

            var files = paths
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .Select(p => p.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();

            if(files.Length == 0)
            {
                return new ImportSummary();
            }

            var summaries = new ConcurrentBag<ImportSummary>();   //colectie thread-safe
            //daca foloseam o lista normala, am fi avut erori de tip „collection modified concurrently”

            var tasks = files.Select(async file =>
            {
                try
                {
                    await logger.WriteLineAsync($"[info] Starting import for file '{file}'...");
                    var s = await ImportFileAsync(file, repository, logger, ct);
                    summaries.Add(s);
                }
                catch (OperationCanceledException)
                {
                    await logger.WriteLineAsync($"[info] Canceled '{file}'.");
                    throw; // propagate so the whole import is canceled
                }
                catch (Exception ex)
                {
                    await logger.WriteLineAsync($"[error] Failed to import file '{file}': {ex.Message}");
                }
            });

            await Task.WhenAll(tasks);

            var total = new ImportSummary();

            foreach(var s in summaries)
            {
                total.Merge(s);
            }

            return total;


        }

    }
}
