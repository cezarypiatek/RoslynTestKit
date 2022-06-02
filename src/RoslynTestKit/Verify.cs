using System;
using System.Linq;
using System.Text;
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

            var workspace = document.Project.Solution.Workspace;
            
            foreach (var operation in operations)
            {
                operation.Apply(workspace, CancellationToken.None);
            }
            var newDocument = workspace.CurrentSolution.GetDocument(document.Id);

            if (newDocument == null)
            {
                throw new InvalidOperationException("Resulting solution does not have the original document");
            }

            var sourceText = newDocument.GetTextAsync(CancellationToken.None).GetAwaiter().GetResult();
            var mergedDocumentBuilder = new StringBuilder();
            mergedDocumentBuilder.Append(sourceText.ToString());
           

            foreach (var doc in newDocument.Project.Documents.OrderByDescending(x=>x.Name))
            {
                if (doc.Id != document.Id)
                {
                    mergedDocumentBuilder.AppendLine($"\r\n{BaseTestFixture.FileSeparator}");
                    mergedDocumentBuilder.Append(doc.GetTextAsync(CancellationToken.None).GetAwaiter().GetResult().ToString());
                }
            }
            var actualCode = mergedDocumentBuilder.ToString();

            if (actualCode != expectedCode)
            {
                DiffHelper.TryToReportDiffWithExternalTool(expectedCode, actualCode);
                var diff = DiffHelper.GenerateInlineDiff(expectedCode, actualCode);
                throw new  TransformedCodeDifferentThanExpectedException(actualCode, expectedCode, diff);
            }
        }
    }
}
