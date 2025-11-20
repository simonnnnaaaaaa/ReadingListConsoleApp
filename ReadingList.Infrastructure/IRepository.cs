
namespace ReadingList.Infrastructure
{
    public interface IRepository<T, TKey> where TKey : notnull
    {
        int Count { get; }

        IEnumerable<TKey> GetKeys {  get; }

        bool Add(T item);

        bool Upsert(T item); 

        bool Remove(TKey key);

        bool TryGet(TKey key, out T? item);

        IEnumerable<T> GetAll();

        void Clear();

    }
}
