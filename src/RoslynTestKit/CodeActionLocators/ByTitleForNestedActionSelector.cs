using System.Collections.Generic;
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
            if (groupSelector.Find(actions) is { } group && NestedCodeActionHelper.TryGetNestedAction(group) is {} nestedActions)
            {
                return nestedActionSelector.Find(nestedActions);
            }

            return null;
        }

        public override string ToString() => $"with nested action [{groupSelector}] -> [{nestedActionSelector}]";
    }
}