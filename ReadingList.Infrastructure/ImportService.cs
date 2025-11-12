using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadingList.Domain;

namespace ReadingList.Infrastructure
{
    public sealed class ImportService
    {

        public async Task<ImportSummary> ImportFileAsync(string path, IRepository<Book, int> repository, TextWriter? log = null)
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

            string[] lines = await File.ReadAllLinesAsync(path).ConfigureAwait(false);

            if (lines.Length == 0)
            {
                log?.WriteLine($"[warn] File '{path}' is empty.");
                return summary;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                
                if(string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var parts = CsvParser.ParseLine(line);

                if(!parts.IsSuccess)
                {
                    log?.WriteLine($"[warn] Line {i + 1} is malformed: {parts.ErrorMessage}. Skipping.");
                    summary.Malformed++;
                    continue;
                }

                var book = parts.Value!;

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


        public async Task<ImportSummary> ImportFilesAsync(IEnumerable<string> paths, IRepository<Book, int> repository, TextWriter?log = null)
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
                    var s = await ImportFileAsync(file, repository, logger);
                    summaries.Add(s);
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
