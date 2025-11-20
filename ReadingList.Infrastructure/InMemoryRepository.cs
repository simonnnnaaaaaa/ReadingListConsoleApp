using System.Collections.Concurrent;


namespace ReadingList.Infrastructure
{
    public class InMemoryRepository<T, TKey> : IRepository<T, TKey> where TKey : notnull
    {
        private readonly ConcurrentDictionary<TKey, T> _store;

        private readonly Func<T, TKey> _keySelector;


        public InMemoryRepository(Func<T, TKey> keySelector, IEqualityComparer<TKey>? keyComparer = null)
        {
            _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
            _store = new ConcurrentDictionary<TKey, T>(keyComparer ?? EqualityComparer<TKey>.Default);
        }

        public int Count => _store.Count;

        public IEnumerable<TKey> GetKeys => _store.Keys;

        public bool Add(T item)
        {
            var key = _keySelector(item);
            return _store.TryAdd(key, item);
        }

        public void Clear()
        {
            _store.Clear();
        }

        public IEnumerable<T> GetAll()
        {
            return _store.Values.ToArray();
        }

        public bool Remove(TKey key)=> _store.TryRemove(key, out _);
        public bool TryGet(TKey key, out T? item)
        {
            var ok = _store.TryGetValue(key, out var found);
            item = ok ? found : default;
            return ok;
        }

        public bool Upsert(T item)
        {
            var key = _keySelector(item);
            _store.AddOrUpdate(key, item, (_, __) => item);
            return true;
        }
    }
}
