using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using RoslynTestKit.Utils;

namespace RoslynTestKit
{
    public static class Verify
    {
        public static void CodeAction(CodeAction codeAction, Document document, string expectedCode)
        {
            var operations = codeAction.GetOperationsAsync(CancellationToken.None).GetAwaiter().GetResult().ToList();
            if (operations.Count == 0)
            {
                throw RoslynTestKitException.NoOperationForCodeAction(codeAction);
            }

            if (operations.Count>1)
            {
                throw RoslynTestKitException.MoreThanOneOperationForCodeAction(codeAction, operations);
            }

            var operation = operations.Single();
            var workspace = document.Project.Solution.Workspace;
            operation.Apply(workspace, CancellationToken.None);

            var newDocument = workspace.CurrentSolution.GetDocument(document.Id);

            var sourceText = newDocument.GetTextAsync(CancellationToken.None).GetAwaiter().GetResult();
            var actualCode = sourceText.ToString();
            if (actualCode != expectedCode)
            {
                DiffHelper.TryToReportDiffWithExternalTool(expectedCode, actualCode);
                var diff = DiffHelper.GenerateInlineDiff(expectedCode, actualCode);
                throw new  TransformedCodeDifferentThanExpectedException(actualCode, expectedCode, diff);
            }
        }
    }
}
