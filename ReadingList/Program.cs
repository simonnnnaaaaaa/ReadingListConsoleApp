using System;
using System.Text;
using System.Threading.Tasks;
using ReadingList.Domain;
using ReadingList.Infrastructure;

namespace ReadingList.App
{
    internal static class Program
    {
        private static readonly IRepository<Book, int> repository =
            new InMemoryRepository<Book, int>(b => b.Id);

        private static readonly ImportService _importService = new ImportService();

        public static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Type 'help' for commands, 'exit' to quit.");

            await RunCommandLoopAsync();
        }

        private static async Task RunCommandLoopAsync()
        {
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line is null) break;

                var cmd = line.Trim();

                if (cmd.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                    cmd.Equals("quit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Goodbye!");
                    break;
                }

                if (cmd.Equals("help", StringComparison.OrdinalIgnoreCase) || cmd == "?")
                {
                    ShowHelp();
                    continue;
                }

                if (cmd.StartsWith("import ", StringComparison.OrdinalIgnoreCase))
                {
                    var path = cmd.Substring("import ".Length).Trim();
                    await HandleImportAsync(path);
                    continue;
                }

                if(cmd.Equals("list all", StringComparison.OrdinalIgnoreCase))
                {
                    var books = repository.GetAll();
                    PrintBooks(books);
                    continue;
                }

                if(cmd.Equals("filter finished", StringComparison.OrdinalIgnoreCase))
                {
                    var books = repository.GetAll().Where(b => b.IsFinished);
                    PrintBooks(books);
                    continue;
                }

                if(cmd.StartsWith("top rated ", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = cmd.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3 && int.TryParse(parts[2], out int n) && n > 0)
                    {
                        var books = repository.GetAll()
                            .OrderByDescending(b => b.Rating)
                            .Take(n);

                        PrintBooks(books);
                    }
                    else
                    {
                        Console.WriteLine("Usage: top rated <n>");
                    }
                    continue;
                }

                if(cmd.StartsWith("by author ", StringComparison.OrdinalIgnoreCase))
                {
                    var keyword = cmd.Substring("by author ".Length).Trim();

                    if(string.IsNullOrWhiteSpace(keyword))
                    {
                        Console.WriteLine("Usage: by author <text>");
                        continue;
                    }
                    else
                    {
                        var books = repository.GetAll()
                            .Where(b => b.Author.Contains(keyword, StringComparison.OrdinalIgnoreCase));

                        PrintBooks(books);
                    }

                    continue;
                }

                Console.WriteLine($"Unknown command: \"{cmd}\"");
                Console.WriteLine("Type 'help' to see available commands.");
                await Task.Yield();
            }
        }

        private static void PrintBooks(IEnumerable<Book> books)
        {
            var bookList = books.ToList();

            if(bookList.Count == 0)
            {
                Console.WriteLine("No books found.");
                return;
            }

            foreach(var book in bookList)
            {
                Console.WriteLine(book.ToString());
            }

        }

        private static void ShowHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("  help                  - show this help");
            Console.WriteLine("  exit                  - quit the app");
            Console.WriteLine("  import <path>         - import books from CSV");
            Console.WriteLine("  list all              - show all imported books");
            Console.WriteLine("  filter finished       - show only finished books");
            Console.WriteLine("  top rated <n>         - show top N books by rating");
            Console.WriteLine("  by author <text>      - show books by author (case-insensitive)");
        }

        private static async Task HandleImportAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine("Usage: import <path-to-csv>");
                return;
            }

            try
            {
                var summary = await _importService.ImportFileAsync(path, repository, Console.Out);
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
