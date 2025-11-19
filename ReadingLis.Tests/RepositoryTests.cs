using ReadingList.Domain;
using ReadingList.Infrastructure;
using Xunit;

namespace ReadingList.Tests;

public class RepositoryTests
{
    [Fact]
    public void Add_Duplicate_ReturnsFalse()
    {
        var repo = new InMemoryRepository<Book, int>(b => b.Id);
        var b = new Book(1, "A", "B", 2000, 100, "x");

        Assert.True(repo.Add(b));
        Assert.False(repo.Add(b));       // duplicate id
        Assert.Equal(1, repo.Count);
    }

    [Fact]
    public void Upsert_Overwrites_Value()
    {
        var repo = new InMemoryRepository<Book, int>(b => b.Id);
        var b = new Book(1, "A", "B", 2000, 100, "x");
        var b2 = new Book(1, "A2", "B", 2000, 100, "x");

        repo.Upsert(b);
        repo.Upsert(b2);

        Assert.True(repo.TryGet(1, out var found));
        Assert.Equal("A2", found!.Title);
    }
}
