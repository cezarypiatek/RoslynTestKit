using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

namespace RoslynNUnitLight
{
    public class RoslynTestKitException: Exception
    {
        public RoslynTestKitException(string message) : base(message)
        {
        }

        public static RoslynTestKitException UnexpectedDiagnostic(string diagnosticId)
        {
            return new RoslynTestKitException($"Found reported diagnostic '{diagnosticId}' in spite of the expectations ");
        }

        public static RoslynTestKitException DiagnosticNotFound(string diagnosticId, IDiagnosticLocator locator, Diagnostic[] reportedDiagnostics)
        {
            var reportedIssues = reportedDiagnostics.Select(x => x.Id).ToList();
            var message = $"There is no issue reported for {diagnosticId} at {locator}. Reported issues: {string.Join(",", reportedIssues)}";
            return new RoslynTestKitException(message);
        }

        public static RoslynTestKitException CodeFixNotFound(int expectedCodeFixIndex, ImmutableArray<CodeAction> codeFixes)
        {
            var message = $"Cannot find CodeFix with index {expectedCodeFixIndex}. Found only {codeFixes.Length} CodeFixes: {string.Join(",", codeFixes.Select((x, index) => $"[{index}] = {x.Title}"))}";
            return new RoslynTestKitException(message);
        }

        public static RoslynTestKitException CodeRefactoringNotFound(int expectedCodeRefactoringIndex, ImmutableArray<CodeAction> codeRefactorings)
        {
            var refactoringDescriptions = GetRefactoringDescriptions(codeRefactorings);
            var message = $"Cannot find CodeRefactoring with index {expectedCodeRefactoringIndex}. Found only {codeRefactorings.Length} CodeRefactorings: {refactoringDescriptions}";
            return new RoslynTestKitException(message);
        }

        public static RoslynTestKitException UnexpectedCodeRefactorings(ImmutableArray<CodeAction> codeRefactorings)
        {
            var refactoringDescriptions = GetRefactoringDescriptions(codeRefactorings);
            return new RoslynTestKitException($"Found reported CodeRefactorings '{refactoringDescriptions}' in spite of the expectations ");
        }

        private static string GetRefactoringDescriptions(ImmutableArray<CodeAction> codeFixes)
        {
            return string.Join(", ", codeFixes.Select((x, index) => $"[{index}] = {x.Title}"));
        }

        public static RoslynTestKitException NoOperationForCodeAction(CodeAction codeAction)
        {
            return new RoslynTestKitException($"There is no operation associated with '{codeAction.Title}'");
        }

        public static RoslynTestKitException MoreThanOneOperationForCodeAction(CodeAction codeAction, List<CodeActionOperation> operations)
        {
            var operationDescriptions = string.Join(", ", operations.Select(x => x.Title));
            return new RoslynTestKitException($"There is more than one operation associated with '{codeAction.Title}'. Found operations: {operationDescriptions}");
        }
    }
}