using System;
using System.IO;
using System.Threading.Tasks;
using ReadingList.Domain;
using ReadingList.Infrastructure;

namespace ReadingList.App.Helpers
{
    public static class CommandHandlers
    {
        public static async Task HandleImportAsync(
            string path,
            IRepository<Book, int> repository,
            ImportService importService,
            TextWriter? log = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine("Usage: import <path-to-csv>");
                return;
            }

            try
            {
                var summary = await importService.ImportFileAsync(path, repository, log ?? Console.Out);

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
    }
}
