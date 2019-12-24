using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteControlToolkitCore.Common.Utilities
{
    public static class DictionaryExtensions
    {
        public static string ShowDictionary<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary)
        {
            if (dictionary.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                var padWidth = dictionary.Max(c => c.Key.ToString().Length) + 5;
                foreach (KeyValuePair<TKey, TValue> c in dictionary)
                {
                    string paddedString = c.Key.ToString().PadRight(padWidth);
                    sb.Append(paddedString)
                        .AppendLine(c.Value.ToString());
                }
                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
        public static string ShowDictionary<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, Func<TValue, string> valueSelector)
        {
            if (dictionary.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                var padWidth = dictionary.Max(c => c.Key.ToString().Length) + 5;
                foreach (KeyValuePair<TKey, TValue> c in dictionary)
                {
                    string paddedString = c.Key.ToString().PadRight(padWidth);
                    sb.Append(paddedString)
                        .AppendLine(valueSelector(c.Value));
                }
                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}