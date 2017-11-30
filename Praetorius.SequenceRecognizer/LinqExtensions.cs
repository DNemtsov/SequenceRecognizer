using System.Collections.Generic;

namespace Praetorius.SequenceRecognizer
{
    public static class LinqExtensions
    {
        public static bool Empty<T>(this ICollection<T> c)
        {
            return c.Count == 0;
        }

        public static bool NotEmpty<T>(this ICollection<T> c)
        {
            return c.Count != 0;
        }

        public static void RemoveLast<T>(this IList<T> l)
        {
            l.RemoveAt(l.Count - 1);
        }

        public static T GetRemoveFirst<T>(this LinkedList<T> l)
        {
            var item = l.First.Value;

            l.RemoveFirst();

            return item;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey k)
            where TValue: new()
        {
            TValue item;

            if (d.TryGetValue(k, out item))
                return item;

            item = new TValue();

            d.Add(k, item);

            return item;
        }
    }
}
