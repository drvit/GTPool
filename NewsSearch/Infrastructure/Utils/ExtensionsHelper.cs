using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace NewsSearch.Infrastructure.Utils
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

    public static class EnumHelper
    {
        public static string ToDescription(this Enum value)
        {
            var da = (DescriptionAttribute[]) (value.GetType().GetField(value.ToString()))
                .GetCustomAttributes(typeof (DescriptionAttribute), false);

            return da.Length > 0 ? da[0].Description : value.ToString();
        }
    }
}