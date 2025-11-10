using System;
using System.Collections.Generic;
using System.Linq;
using ReadingList.Domain;

namespace ReadingList.App.Helpers
{

    public static class ConsolePrinter
    {

        public static void ShowHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("  help                  - show this help");
            Console.WriteLine("  exit                  - quit the app");
            Console.WriteLine("  import <path>         - import books from CSV");
            Console.WriteLine("  list all              - show all imported books");
            Console.WriteLine("  filter finished       - show only finished books");
            Console.WriteLine("  top rated <n>         - show top N books by rating");
            Console.WriteLine("  by author <text>      - show books by author (case-insensitive)");
            Console.WriteLine("  stats                 - show statistics");
            Console.WriteLine("  mark finished <id>    - mark a book as finished");
            Console.WriteLine("  rate <id> <0-5>       - update a book rating");
        }

        public static void PrintBooks(IEnumerable<Book> books)
        {
            var bookList = books.ToList();

            if (bookList.Count == 0)
            {
                Console.WriteLine("No books found.");
                return;
            }

            foreach (var book in bookList)
            {
                Console.WriteLine(book.ToString());
            }

        }

        public static void PrintStats(IEnumerable<Book> books)
        {
            Console.WriteLine("=== Reading List Statistics ===");

            var bookList = books.ToList();

            Console.WriteLine($"\nTotal books: {bookList.Count}");

            int finished = bookList.Count(b => b.IsFinished);

            Console.WriteLine($"\nFinished books: {finished}");

            var avgRating = bookList.Any()
                ? bookList.Average(b => b.Rating)
                : 0.0;

            Console.WriteLine($"\nAverage rating: {avgRating:0.00}");

            var pagesByGenre = bookList
                .GroupBy(b => b.Genre)
                .Select(g => new { Genre = g.Key, NumberOfPages = g.Sum(b => b.NumberOfPages) })
                .ToList();

            Console.WriteLine("\nTotal pages by genre:");
            if (pagesByGenre.Count == 0)
            {
                Console.WriteLine("  (no data)");
            }
            else
            {
                foreach (var pg in pagesByGenre)
                {
                    Console.WriteLine($"- {pg.Genre}: {pg.NumberOfPages}");
                }

            }

            var topAuthors = bookList
                .GroupBy(b => b.Author)
                .Select(g => new { Author = g.Key, BookCount = g.Count() })
                .OrderByDescending(a => a.BookCount)
                .Take(3)
                .ToList();

            Console.WriteLine("\nTop 3 authors by number of books:");
            if (topAuthors.Count == 0)
            {
                Console.WriteLine("  (no data)");
            }
            else
            {
                foreach (var author in topAuthors)
                {
                    Console.WriteLine($"- {author.Author}: {author.BookCount}");
                }
            }

        }
    }
}
