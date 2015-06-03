using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsSearch.Infrastructure
{
    public static class ExtensionsHelper
    {
        public static object GetValueForKey(this Dictionary<string, object> dictionary, string key)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : null;
        }

        public static IEnumerable<T> Add<T>(this IEnumerable<T> e, T value)
        {
            foreach (var cur in e)
            {
                yield return cur;
            }
            yield return value;
        }
    }
}