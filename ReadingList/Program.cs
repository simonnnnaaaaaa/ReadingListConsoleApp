using System;
using System.Text;
using System.Threading.Tasks;
using ReadingList.Domain;
using ReadingList.Infrastructure;
using ReadingList.App.Helpers;

namespace ReadingList.App
{
    internal static class Program
    {
        private static readonly IRepository<Book, int> repository =
            new InMemoryRepository<Book, int>(b => b.Id);

        private static readonly ImportService _importService = new ImportService();
        private static readonly ExportService _exportService = new ExportService();

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
                    ConsolePrinter.ShowHelp();
                    continue;
                }

                if (cmd.StartsWith("import ", StringComparison.OrdinalIgnoreCase))
                {

                    var arg = cmd.Substring("import ".Length).Trim();

                    var parts = CommandHandlers.SplitArgs(arg);

                    if(parts.Length == 1)
                    {
                        await CommandHandlers.HandleImportAsync(parts[0], repository,_importService, Console.Out);
                    }
                    else
                    {
                        await CommandHandlers.HandleImportManyAsync(parts, repository, _importService, Console.Out);
                    }
                    continue;
                }

                if(cmd.Equals("list all", StringComparison.OrdinalIgnoreCase))
                {
                    var books = repository.GetAll();
                    ConsolePrinter.PrintBooks(books);
                    continue;
                }

                if(cmd.Equals("filter finished", StringComparison.OrdinalIgnoreCase))
                {
                    var books = repository.GetAll().Where(b => b.IsFinished);
                    ConsolePrinter.PrintBooks(books);
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

                        ConsolePrinter.PrintBooks(books);
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

                        ConsolePrinter.PrintBooks(books);
                    }

                    continue;
                }

                if(cmd.Equals("stats", StringComparison.OrdinalIgnoreCase))
                {
                    var books = repository.GetAll();
                    ConsolePrinter.PrintStats(books);
                    continue;
                }

                if (cmd.StartsWith("mark finished ", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = cmd.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length == 3 && int.TryParse(parts[2], out int id) && id > 0)
                    {
                        await CommandHandlers.HandleMarkFinishedAsync(id, repository);
                    }
                    else
                    {
                        Console.WriteLine("Usage: mark finished <book-id>");
                    }
                    continue;
                }

                if (cmd.StartsWith("rate ", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = cmd.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length == 3 
                        && int.TryParse(parts[1], out int id)
                        && id > 0
                        && double.TryParse(parts[2], 
                                           System.Globalization.NumberStyles.Float, 
                                           System.Globalization.CultureInfo.InvariantCulture, 
                                           out double rating)
                        )
                    {
                        await CommandHandlers.HandleRateAsync(id, rating, repository);
                    }
                    else
                    {
                        Console.WriteLine("Usage: rate <id> <0-5>");
                    }
                    continue;
                }

                if(cmd.StartsWith("export json ", StringComparison.OrdinalIgnoreCase))
                {
                    var path = cmd.Substring("export json ".Length).Trim();
                    await CommandHandlers.HandleExportJsonAsync(path, repository, _exportService);
                    continue;
                }

                if(cmd.StartsWith("export csv ", StringComparison.OrdinalIgnoreCase))
                {
                    var path = cmd.Substring("export csv ".Length).Trim();
                    await CommandHandlers.HandleExportCsvAsync(path, repository, _exportService);
                    continue;
                }

                Console.WriteLine($"Unknown command: \"{cmd}\"");
                Console.WriteLine("Type 'help' to see available commands.");
                await Task.Yield();
            }
        }

        

    }
}
