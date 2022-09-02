using System;
using System.Collections.Generic;
using System.Text;

namespace GCloudShared.Extensions
{
    public static class EnumerableExtensions
    {
        public static string ToCommaSeparatedString(this IEnumerable<string> enumerable)
        {
            return string.Join(",", enumerable);
        }

        public static string ToCommaSeparatedString(this IEnumerable<string> enumerable, string separator)
        {
            return string.Join(separator, enumerable);
        }
    }
}
