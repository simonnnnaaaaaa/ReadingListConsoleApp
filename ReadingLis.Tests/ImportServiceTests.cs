using ReadingList.Domain;
using ReadingList.Infrastructure;
using System.Text;
using Xunit;

namespace ReadingList.Tests;

public class ImportServiceTests
{
    private static string MakeCsv(params string[] lines)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,Title,Author,Year,Pages,Genre,Finished,Rating");
        foreach (var l in lines) sb.AppendLine(l);
        return sb.ToString();
    }

    [Fact]
    public async Task ImportFiles_Parallel_Combines_SkipsDuplicates_LogsMalformed()
    {
        // Arrange
        var dir = Directory.CreateTempSubdirectory();
        var f1 = Path.Combine(dir.FullName, "a.csv");
        var f2 = Path.Combine(dir.FullName, "b.csv");

        // f1: 2 valide, 1 malformed
        await File.WriteAllTextAsync(f1, MakeCsv(
            @"1,""Clean Code"",Robert C. Martin,2008,464,software,yes,5",
            @"2,The Hobbit,J.R.R. Tolkien,1937,310,fantasy,no,4.5",
            @"BAD,LINE,THERE"
        ));

        // f2: 1 valid (duplicate id=2), 1 valid (id=3)
        await File.WriteAllTextAsync(f2, MakeCsv(
            @"2,Duplicate,J.R.R. Tolkien,1937,300,fantasy,no,4.4",
            @"3,Refactoring,Martin Fowler,1999,448,software,yes,4.7"
        ));

        var repo = new InMemoryRepository<Book, int>(b => b.Id);
        var svc = new ImportService(delayMs: 0);
        using var log = new StringWriter();

        // Act
        var summary = await svc.ImportFilesAsync(new[] { f1, f2 }, repo, log);

        // Assert
        Assert.Equal(3, repo.Count);            // ids: 1,2,3 (dup 2 kept once)
        Assert.Equal(3, summary.Imported);      // 1,2,3 valid unique
        Assert.Equal(1, summary.Duplicates);    // duplicate id=2
        Assert.Equal(1, summary.Malformed);     // BAD line

        var logText = log.ToString();
        Assert.Contains("malformed", logText, StringComparison.OrdinalIgnoreCase);

        // Cleanup
        Directory.Delete(dir.FullName, recursive: true);
    }
}
