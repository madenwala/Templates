using System;
using System.Collections.Generic;
using System.Linq;

namespace AppFramework.Core.Extensions
{
    public static class IDictionaryExtensions
    {
        /// <summary>
        /// Checks whether or not a string IDictionary contains a particular key and value combination.
        /// </summary>
        /// <param name="dic">String IDictionary instance.</param>
        /// <param name="key">Key to check for.</param>
        /// <param name="value">Value to check for.</param>
        /// <returns>True if it contains the combination else false.</returns>
        public static bool ContainsKeyAndValue(this IDictionary<string, string> dic, string key, string value)
        {
            return dic.ContainsKey(key) && dic[key].Equals(value, StringComparison.CurrentCultureIgnoreCase);
        }

        public static IEnumerable<KeyValuePair<T, S>> ToEnumerableArray<T, S>(this IDictionary<T, S> dictionary)
        {
            var list = new List<KeyValuePair<T, S>>();
            foreach (var pair in dictionary)
                list.Add(pair);
            return list;
        }

        public static string ToQueryString<T, S>(this IDictionary<T, S> dictionary)
        {
            string querystring = string.Empty;
            List<string> parameters = new List<string>();

            if (dictionary == null)
                return querystring;

            foreach (var pair in dictionary.Where(w => w.Key?.ToString() != null && w.Value?.ToString() != null))
                parameters.Add(string.Join("=", pair.Key.ToString(), Uri.EscapeUriString(pair.Value.ToString())));

            if (parameters.Count > 0)
                querystring = "?" + string.Join("&", parameters);

            return querystring;
        }
    }
}