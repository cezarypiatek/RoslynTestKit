using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using ApprovalTests.Reporters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

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
            var text = sourceText.ToString();
            if (text != expectedCode)
            {
                TryToReportDiff(expectedCode, text);
                throw new  TransformedCodeDifferentThanExpectedException(text, expectedCode);
            }
        }

        private static void TryToReportDiff(string expectedCode, string text)
        {
            if (Debugger.IsAttached)
            {
                var tmpDir = Path.GetTempPath();
                var tempFileName = Guid.NewGuid().ToString("N").Substring(0, 4);
                var transformedPath = Path.Combine(tmpDir, $"{tempFileName}_transformed.cs");
                File.WriteAllText(transformedPath, text);
                var expectedPath = Path.Combine(tmpDir, $"{tempFileName}_expected.cs");
                File.WriteAllText(expectedPath, expectedCode);
                new DiffReporter().Report( transformedPath, expectedPath);
            }
        }
    }
}
