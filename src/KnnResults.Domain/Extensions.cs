using System.Collections.Generic;
using System.Linq;

namespace KnnResults.Domain
{
    public static class Extensions
    {
        public static IDictionary<K, V> Reverse<K, V>(this IDictionary<V, K> original)
        {
            return original.ToDictionary(x => x.Value, x => x.Key);
        }
    }
}
