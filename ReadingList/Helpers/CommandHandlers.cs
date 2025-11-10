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

    }
}
