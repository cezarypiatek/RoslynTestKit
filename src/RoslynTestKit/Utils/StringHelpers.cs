using System;
using System.Collections.Generic;
using System.Linq;

namespace RoslynTestKit.Utils
{
    internal static class StringHelpers
    {
        public static string MergeWithComma<T>(this IReadOnlyList<T> list, Func<T, string> map=null)
        {
            return string.Join(", ", list.Select(map ?? (x => x.ToString())));
        }

        public static string MergeAsBulletList<T>(this IReadOnlyList<T> list, Func<T, string> map=null)
        {
            var mapItem = map ?? (x => x.ToString());
            return string.Join(Environment.NewLine, list.Select(x => $"- {mapItem(x)}"));
        }
    }
}