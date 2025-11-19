using ReadingList.Domain;
using ReadingList.Infrastructure;
using System.Globalization;
using Xunit;

namespace ReadingList.Tests;

public class CsvParserTests
{
    [Fact]
    public void ParseLine_Valid_MapsProperties()
    {
        // InvariantCulture numbers; Finished "yes"
        var line = @"1,""Clean Code"",Robert C. Martin,2008,464,software,yes,4.5";
        var result = CsvParser.ParseLine(line);

        Assert.True(result.IsSuccess);
        var b = result.Value!;
        Assert.Equal(1, b.Id);
        Assert.Equal("Clean Code", b.Title);
        Assert.Equal("Robert C. Martin", b.Author);
        Assert.Equal(2008, b.YearPublished);
        Assert.Equal(464, b.NumberOfPages);
        Assert.Equal("software", b.Genre, StringComparer.OrdinalIgnoreCase);
        Assert.True(b.IsFinished);
        Assert.Equal(4.5, b.Rating, precision: 10);
    }

    [Theory]
    [InlineData(",,Author,2000,100,genre,yes,4.0")] // missing id & title
    [InlineData("1,Title,,2000,100,genre,yes,4.0")] // missing author
    [InlineData("1,Title,Author,notYear,100,genre,yes,4.0")] // bad year
    [InlineData("1,Title,Author,2000,notInt,genre,yes,4.0")] // bad pages
    [InlineData("1,Title,Author,2000,100,genre,yes,9.9")] // rating out of range
    public void ParseLine_Malformed_Fails(string line)
    {
        var result = CsvParser.ParseLine(line);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Null(result.Value);
    }
}
