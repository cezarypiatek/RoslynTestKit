using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Completion;
using RoslynTestKit.Utils;

namespace RoslynTestKit
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
            var message = $"There is no issue reported for {diagnosticId} at {locator}. Reported issues: {reportedDiagnostics.MergeWithComma(x=>x.Id)}";
            return new RoslynTestKitException(message);
        }

        public static RoslynTestKitException CodeFixNotFound(int expectedCodeFixIndex, ImmutableArray<CodeAction> codeFixes)
        {
            var message = $"Cannot find CodeFix with index {expectedCodeFixIndex}. Found only {codeFixes.Length} CodeFixes: {GetActionsDescription(codeFixes)}";
            return new RoslynTestKitException(message);
        }

        public static RoslynTestKitException CodeRefactoringNotFound(int expectedCodeRefactoringIndex, ImmutableArray<CodeAction> codeRefactorings)
        {
            var refactoringDescriptions = GetActionsDescription(codeRefactorings);
            var message = $"Cannot find CodeRefactoring with index {expectedCodeRefactoringIndex}. Found only {codeRefactorings.Length} CodeRefactorings: {refactoringDescriptions}";
            return new RoslynTestKitException(message);
        }

        public static RoslynTestKitException UnexpectedCodeRefactorings(ImmutableArray<CodeAction> codeRefactorings)
        {
            var refactoringDescriptions = GetActionsDescription(codeRefactorings);
            return new RoslynTestKitException($"Found reported CodeRefactorings '{refactoringDescriptions}' in spite of the expectations ");
        }

        private static string GetActionsDescription(ImmutableArray<CodeAction> codeFixes)
        {
            return string.Join(", ", codeFixes.Select((x, index) => $"[{index}] = {x.Title}"));
        }

        public static RoslynTestKitException NoOperationForCodeAction(CodeAction codeAction)
        {
            return new RoslynTestKitException($"There is no operation associated with '{codeAction.Title}'");
        }

        public static RoslynTestKitException MoreThanOneOperationForCodeAction(CodeAction codeAction, List<CodeActionOperation> operations)
        {
            return new RoslynTestKitException($"There is more than one operation associated with '{codeAction.Title}'. Found operations: {operations.MergeWithComma(x=>x.Title)}");
        }

        public static Exception CannotFindSuggestion(IReadOnlyList<string> missingCompletion, ImmutableArray<CompletionItem> resultItems)
        {
            return new RoslynTestKitException($"Cannot get suggestions:\r\n{missingCompletion.MergeAsBulletList()}\r\nFound suggestions:\r\n{resultItems.MergeAsBulletList(x=>x.DisplayText)}");
        }
    }
}