using System;
using System.Collections.Generic;

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
    }
}