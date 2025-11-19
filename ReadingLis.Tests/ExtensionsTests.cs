using ReadingList.Domain;
using Xunit;

namespace ReadingList.Tests;

public class ExtensionsTests
{
    [Fact]
    public void AverageRatingOrDefault_Empty_ReturnsZero()
    {
        var empty = Enumerable.Empty<Book>();
        Assert.Equal(0.0, empty.AverageRatingOrDefault(), precision: 10);
    }

    [Fact]
    public void TopRated_ReturnsExpectedOrder()
    {
        var books = new[]
        {
            new Book(1, "A", "X", 2000, 100, "g", rating: 4.1),
            new Book(2, "B", "X", 2000, 100, "g", rating: 4.9),
            new Book(3, "C", "X", 2000, 100, "g", rating: 3.0),
            new Book(4, "D", "X", 2000, 100, "g", rating: 4.9) // tie, will then sort by Title
        };

        var top = books.TopRated(3).ToList();
        Assert.Equal(new[] { 2, 4, 1 }, top.Select(b => b.Id)); // 4.9 (B), 4.9 (D), 4.1 (A)
    }
}
