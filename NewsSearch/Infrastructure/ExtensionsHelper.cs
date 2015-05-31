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
    }
}