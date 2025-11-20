using ReadingList.Domain;

namespace ReadingList.Infrastructure.Exporting
{ 
    public interface IExportStrategy
    {
        string Name { get; }

        Task ExportAsync(IEnumerable<Book> books, string path, CancellationToken ct = default);
    }
}
