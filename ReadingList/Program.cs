using System;
using System.Text;
using System.Threading.Tasks;
using ReadingList.Domain;
using ReadingList.Infrastructure;

namespace ReadingList.App
{
    internal static class Program
    {
        private static readonly IRepository<Book, int> _repo =
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
                    Console.WriteLine("Available commands:");
                    Console.WriteLine("  help                 - show this help");
                    Console.WriteLine("  exit                 - quit the app");
                    Console.WriteLine("  import <path>        - import books from CSV");
                    continue;
                }

                if (cmd.StartsWith("import ", StringComparison.OrdinalIgnoreCase))
                {
                    var path = cmd.Substring("import ".Length).Trim();
                    await HandleImportAsync(path);
                    continue;
                }

                Console.WriteLine($"Unknown command: \"{cmd}\"");
                Console.WriteLine("Type 'help' to see available commands.");
                await Task.Yield();
            }
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
                var summary = await _importService.ImportFileAsync(path, _repo, Console.Out);
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
