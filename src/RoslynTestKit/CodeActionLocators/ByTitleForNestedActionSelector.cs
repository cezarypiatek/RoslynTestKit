using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis.CodeActions;

namespace RoslynTestKit.CodeActionLocators
{
    /// <summary>
    ///     This selector is intended to search for a nested code actions
    /// </summary>
    public class ByTitleForNestedActionSelector: ICodeActionSelector
    {
        private readonly ByTitleCodeActionSelector groupSelector;
        private readonly ByTitleCodeActionSelector nestedActionSelector;

        public ByTitleForNestedActionSelector(string groupTitle, string actionTitle)
        {
            this.groupSelector = new ByTitleCodeActionSelector(groupTitle);
            this.nestedActionSelector = new ByTitleCodeActionSelector(actionTitle);
        }

        public CodeAction Find(IReadOnlyList<CodeAction> actions)
        {
            if (groupSelector.Find(actions) is { } group && group.GetType() is {Name: "CodeActionWithNestedActions"} groupType)
            {
                var nestedCodeActionObj = groupType.GetProperty("NestedCodeActions", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.GetValue(group);
                if (nestedCodeActionObj != null)
                {
                    var nestedActions = (ImmutableArray<CodeAction>) nestedCodeActionObj;
                    return nestedActionSelector.Find(nestedActions);
                }
            }

            return null;
        }
    }
}