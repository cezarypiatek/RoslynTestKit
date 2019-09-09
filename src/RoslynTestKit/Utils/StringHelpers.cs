using System;
using System.Collections.Generic;
using System.Linq;

namespace RoslynTestKit.Utils
{
    internal static class StringHelpers
    {
        public static string MergeWithComma<T>(this IReadOnlyList<T> list, Func<T, string> map=null, string title=null)
        {
            if (list.Count == 0)
            {
                return string.Empty;
            }
            return title + string.Join(", ", list.Select(map ?? (x => x.ToString())));
        }

        public static string MergeAsBulletList<T>(this IReadOnlyList<T> list, Func<T, string> map=null, string title = null)
        {
            if (list.Count == 0)
            {
                return string.Empty;
            }
            var mapItem = map ?? (x => x.ToString());
            return title + string.Join(Environment.NewLine, list.Select(x => $"- {mapItem(x)}"));
        }
    }
}