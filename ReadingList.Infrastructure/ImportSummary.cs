
namespace ReadingList.Infrastructure
{
    public sealed class ImportSummary
    {
        public int Imported { get; set; }
        public int Duplicates { get; set; }
        public int Malformed { get; set; }

        public List<int> SkippedIds { get; } = new();

        public void Merge(ImportSummary other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }
            
            Imported += other.Imported;
            Duplicates += other.Duplicates;
            Malformed += other.Malformed;
            
            if(other.SkippedIds.Count > 0)
            {
                SkippedIds.AddRange(other.SkippedIds);
            }
        }
    }
}
