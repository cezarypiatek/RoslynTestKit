using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeActions;

namespace RoslynTestKit.CodeActionLocators
{
    public interface ICodeActionSelector
    {
        public CodeAction Find(IReadOnlyList<CodeAction> actions);
    }
}
