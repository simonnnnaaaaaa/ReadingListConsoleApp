using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingList.Infrastructure
{
    public sealed class ImportSummary
    {
        public int Imported { get; set; }
        public int Duplicates { get; set; }
        public int Malformed { get; set; }

        //IDs that were skipped because they already existed
        public List<int> SkippedIds { get; } = new();

        public override string ToString()
           => $"Imported={Imported}, Duplicates={Duplicates}, Malformed={Malformed}";


    }
}
