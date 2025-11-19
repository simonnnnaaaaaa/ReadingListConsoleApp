using ReadingList.Domain;
using Xunit;

namespace ReadingList.Tests;

public class BookTests
{
    [Fact]
    public void SetRating_OutOfRange_Throws()
    {
        var b = new Book(1, "A", "B", 2000, 100, "x");
        Assert.Throws<ArgumentOutOfRangeException>(() => b.SetRating(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => b.SetRating(5.1));
    }

    [Fact]
    public void SetRating_Valid_SetsAndRoundsTo1Decimal()
    {
        var b = new Book(1, "A", "B", 2000, 100, "x");
        b.SetRating(4.56);
        Assert.Equal(4.6, b.Rating, precision: 10);
    }
}
