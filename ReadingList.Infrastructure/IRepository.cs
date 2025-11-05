using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingList.Infrastructure
{
    public interface IRepository<T, TKey>
    {
        int Count { get; }

        IEnumerable<TKey> GetKeys {  get; }

        bool Add(T item);

        bool Upsert(T item); //insert if missing, overwrite if present. Always returns true

        bool Remove(TKey key);

        bool TryGet(TKey key, out T? item);

        IEnumerable<T> GetAll();

        void Clear();

    }
}
