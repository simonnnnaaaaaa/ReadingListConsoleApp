using System.Collections.Concurrent;
using ReadingList.Domain;

namespace ReadingList.Infrastructure
{
    public sealed class ImportService
    {
        private readonly int _delayMs;
        private readonly IRepository<Book, int> _repository;
        private readonly TextWriter _log;

        public ImportService(IRepository<Book, int> repository, TextWriter? log = null, int delayMs = 800)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _log = log ?? TextWriter.Null;
            if (delayMs < 0) throw new ArgumentOutOfRangeException(nameof(delayMs));
            _delayMs = delayMs;
        }

        public async Task<ImportSummary> ImportFileAsync(string path, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            }

            var summary = new ImportSummary();

            string[] lines = await File.ReadAllLinesAsync(path, ct).ConfigureAwait(false);

            if (lines.Length == 0)
            {
                await _log?.WriteLineAsync($"[warn] File '{path}' is empty.");
                return summary;
            }

            var headerLine = lines[0].Trim().TrimStart('\uFEFF');
            var headerCheck = CsvParser.ValidateHeader(headerLine);
            if (!headerCheck.IsSuccess)
            {
                await _log?.WriteLineAsync($"[warn] {headerCheck.ErrorMessage} File '{path}' skipped.");
                return summary;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                ct.ThrowIfCancellationRequested(); 

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
                    await _log?.WriteLineAsync($"[warn] {Path.GetFileName(path)}: line {i + 1} malformed: {parsed.ErrorMessage}. Skipping.");
                    summary.Malformed++;
                    continue;
                }

                var book = parsed.Value!;

                bool added = _repository.Add(book);

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


        public async Task<ImportSummary> ImportFilesAsync(IEnumerable<string> paths, CancellationToken ct = default)
        {
            var files = paths
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .Select(p => p.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();

            if(files.Length == 0)
            {
                return new ImportSummary();
            }

            var summaries = new ConcurrentBag<ImportSummary>();  

            var tasks = files.Select(async file =>
            {
                try
                {
                    await _log.WriteLineAsync($"[info] Starting import for file '{file}'...");
                    var s = await ImportFileAsync(file, ct);
                    summaries.Add(s);
                }
                catch (OperationCanceledException)
                {
                    await _log.WriteLineAsync($"[info] Canceled '{file}'.");
                    throw; 
                }
                catch (Exception ex)
                {
                    await _log.WriteLineAsync($"[error] Failed to import file '{file}': {ex.Message}");
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
