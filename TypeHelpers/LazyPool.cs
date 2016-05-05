using System;
using System.Collections.Concurrent;

static class LazyPool
{
    static class OfType<T>
    {
        public static readonly ConcurrentDictionary<T, Lazy<T>> Pool = new ConcurrentDictionary<T, Lazy<T>>();
    }

    public static Lazy<T> ToLazy<T>(this T value)
    {
        Lazy<T> tmp;
        var pool = OfType<T>.Pool;
        if (pool.TryGetValue(value, out tmp))
            return tmp;
        return pool.GetOrAdd(value, new Lazy<T>(() => value));
    }
}
