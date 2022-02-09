using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis.CodeActions;

namespace RoslynTestKit.CodeActionLocators
{
    internal static class NestedCodeActionHelper
    {
        public static ImmutableArray<CodeAction>? TryGetNestedAction(CodeAction group)
        {
            if (group.GetType() is {Name: "CodeActionWithNestedActions"} groupType)
            {
                var nestedCodeActionObj = groupType.GetProperty("NestedCodeActions", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.GetValue(group);
                if (nestedCodeActionObj != null)
                {
                    return  (ImmutableArray<CodeAction>)nestedCodeActionObj;
                }
            }

            return null;
        }
    }
}