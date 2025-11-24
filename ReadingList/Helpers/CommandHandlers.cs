using ReadingList.Domain;
using ReadingList.Infrastructure;
using System.Text;
using ReadingList.Infrastructure.Exporting;


namespace ReadingList.App.Helpers
{
    public static class CommandHandlers
    {
        
        public static async Task HandleImportAsync(string path,  ImportService importService, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine("Usage: import <path-to-csv>");
                return;
            }

            try
            {
                var summary = await importService.ImportFileAsync(path, ct);

                Console.WriteLine();
                Console.WriteLine("=== Import summary ===");
                Console.WriteLine($"Imported : {summary.Imported}");
                Console.WriteLine($"Duplicates: {summary.Duplicates}");
                Console.WriteLine($"Malformed : {summary.Malformed}");
                if (summary.SkippedIds.Count > 0)
                    Console.WriteLine("Skipped Ids: " + string.Join(", ", summary.SkippedIds));
                Console.WriteLine("======================");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[error] Import failed: {ex.Message}");
            }
        }

        public static async Task HandleImportManyAsync(string[] paths, ImportService importService, CancellationToken ct = default)
        {
            if(paths.Length == 0)
            {
                Console.WriteLine("Usage: import files");
                return;
            }

            try
            {
                var summary = await importService.ImportFilesAsync(paths, ct);

                Console.WriteLine("\n=== Total import summary ===");
                Console.WriteLine($"Imported : {summary.Imported}");
                Console.WriteLine($"Duplicates: {summary.Duplicates}");
                Console.WriteLine($"Malformed : {summary.Malformed}");

                if(summary.SkippedIds.Count > 0)
                {
                    Console.WriteLine("Skipped Ids: " + string.Join(", ", summary.SkippedIds));
                }
                Console.WriteLine("============================\n");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[error] Import failed: {ex.Message}");
            }
        }


        public static Task HandleMarkFinishedAsync(int id, IRepository<Book, int> repository)
        {
            if(!repository.TryGet(id, out var book) || book is null)
            {
                Console.WriteLine($"[error] Book with Id {id} not found.");
                return Task.CompletedTask;
            }

            if(!book.IsFinished)
            {
                book.MarkAsFinished();
                repository.Upsert(book);
                Console.WriteLine($"Book '{book.Title}' marked as finished.");
            }
            else
            {
                Console.WriteLine($"Book '{book.Title}' is already marked as finished.");
            }

            return Task.CompletedTask;
        }

        public static Task HandleRateAsync(int id, double rating, IRepository<Book, int> repository)
        {
            if (rating < 0.0 || rating > 5.0)
            {
                Console.WriteLine("[error] Rating must be between 0.0 and 5.0.");
                return Task.CompletedTask;
            }

            if (!repository.TryGet(id, out var book) || book is null)
            {
                Console.WriteLine($"[error] Book with Id {id} not found.");
                return Task.CompletedTask;
            }

            try
            {
                book.SetRating(rating);
                repository.Upsert(book);
                Console.WriteLine($"Book '{book.Title}' rated {book.Rating}.");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"[error] {ex.Message}");
            }

            return Task.CompletedTask;


        }


        public static async Task HandleExportJsonAsync(string path, IRepository<Book, int> repository, ExportService exportService, CancellationToken ct)
        {
            if(string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine("Usage: export json <path>");
                return;
            }

            if(File.Exists(path))
            {
                Console.Write("File exists. Overwrite? (y/n): ");
                var answer = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();
                if (answer is not ("y" or "yes"))
                {
                    Console.WriteLine("Export canceled.");
                    return;
                }
            }

            try
            {
                await exportService.ExportAsync("json", repository.GetAll(), path, ct);
                Console.WriteLine($"Exported books to JSON file at '{path}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[error] Export JSON failed: {ex.Message}");
            }

        }

        public static async Task HandleExportCsvAsync(string path, IRepository<Book, int> repository, ExportService exportService, CancellationToken ct = default)
        {

            if(string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine("Usage: export csv <path>");
                return;
            }

            if (File.Exists(path))
            {
                Console.Write("File exists. Overwrite? (y/n): ");
                var answer = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();
                if (answer is not ("y" or "yes"))
                {
                    Console.WriteLine("Export canceled.");
                    return;
                }
            }

            try
            {
                await exportService.ExportAsync("csv", repository.GetAll(), path, ct);
                Console.WriteLine($"Exported CSV to: {path}");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[info] Export canceled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[error] Export CSV failed: {ex.Message}");
            }

        }

        public static string[] SplitArgs(string input)
        {
            var args = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;

            foreach(var ch in input)
            {
                if (ch == '"') 
                { 
                    inQuotes = !inQuotes; 
                    continue; 
                }

                if(!inQuotes && char.IsWhiteSpace(ch))
                {
                    if(sb.Length > 0)
                    {
                        args.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else
                {
                    sb.Append(ch);
                }
            }

            if(sb.Length > 0)
            {
                args.Add(sb.ToString());
            }

            return args.ToArray();

        }

    }
}
