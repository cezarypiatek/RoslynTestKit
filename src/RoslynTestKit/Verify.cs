using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
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
            var text = ConvertToLineEndingsAwareString(sourceText);

            mergedDocumentBuilder.Append(text);


            foreach (var doc in newDocument.Project.Documents.OrderByDescending(x => x.Name))
            {
                if (doc.Id != document.Id)
                {
                    mergedDocumentBuilder.AppendLine($"{Environment.NewLine}{BaseTestFixture.FileSeparator}");

                    var docSourceText = doc.GetTextAsync(CancellationToken.None).GetAwaiter().GetResult();
                    var docText = ConvertToLineEndingsAwareString(sourceText);

                    mergedDocumentBuilder.Append(docText);
                }
            }
            var actualCode = mergedDocumentBuilder.ToString();

            if (actualCode != expectedCode)
            {
                DiffHelper.TryToReportDiffWithExternalTool(expectedCode, actualCode);
                var diff = DiffHelper.GenerateInlineDiff(expectedCode, actualCode);
                throw new TransformedCodeDifferentThanExpectedException(actualCode, expectedCode, diff);
            }
        }

        private static string ConvertToLineEndingsAwareString(SourceText sourceText)
        {
            string text = sourceText.ToString();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                text = text.Replace("\r\n", "\n");
            }

            return text;
        }
    }
}
