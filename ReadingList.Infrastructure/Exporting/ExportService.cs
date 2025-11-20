using ReadingList.Domain;

namespace ReadingList.Infrastructure.Exporting
{
    public sealed class ExportService
    {
        private readonly Dictionary<string, IExportStrategy> _strategies;

        public ExportService(IEnumerable<IExportStrategy> strategies)
        {
            _strategies = strategies.ToDictionary(s => s.Name.ToLowerInvariant());
        }

        public IReadOnlyCollection<string> SupportedFormats => _strategies.Keys;

        public Task ExportAsync(string format, IEnumerable<Book> books, string path, CancellationToken ct = default)
        {
            if (!_strategies.TryGetValue(format.ToLowerInvariant(), out var strategy))
                throw new ArgumentException($"Unknown export format '{format}'. Supported: {string.Join(", ", _strategies.Keys)}");

            return strategy.ExportAsync(books, path, ct);
        }
    }
}
