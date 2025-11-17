using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadingList.Domain
{
    public static class BookExtensions
    {

        public static double AverageRatingOrDefault(this IEnumerable<Book> books)
            => books.Any() ? books.Average(b => b.Rating) : 0.0;

        public static IEnumerable<Book> FilterFinished(this IEnumerable<Book> books)
            => books.Where(b => b.IsFinished);

        public static IEnumerable<Book> ByAuthorContains(this IEnumerable<Book> books, string text)
            => books.Where(b => (b.Author ?? string.Empty).ContainsCaseInsensitive(text));

        public static IEnumerable<Book> TopRated(this IEnumerable<Book> books, int n)
            => books.OrderByDescending(b => b.Rating).ThenBy(b => b.Title).Take(n);

        public static IEnumerable<(string Genre, int TotalPages)> PagesByGenre(this IEnumerable<Book> books)
            => books
                .GroupBy(b => b.Genre ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .Select(g => (Genre: g.Key, TotalPages: g.Sum(b => b.NumberOfPages)))
                .OrderByDescending(x => x.TotalPages).ThenBy(x => x.Genre);

        public static IEnumerable<(string Author, int Count)> TopAuthorsByCount(this IEnumerable<Book> books, int n)
            => books
                .GroupBy(b => b.Author ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .Select(g => (Author: g.Key, Count: g.Count()))
                .OrderByDescending(x => x.Count).ThenBy(x => x.Author)
                .Take(n);

    }
}