﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Completion;
using RoslynTestKit.Utils;

namespace RoslynTestKit
{
    public class RoslynTestKitException : Exception
    {
        public RoslynTestKitException(string message) : base(message)
        {
        }

        public static RoslynTestKitException UnexpectedDiagnostic(string diagnosticId, IDiagnosticLocator locator = null)
        {
            var description = locator != null ? $"at {locator.Description()}" : string.Empty;
            return new RoslynTestKitException($"Found reported diagnostic '{diagnosticId}' in spite of the expectations {description}");
        }

        internal static Exception ExceptionInAnalyzer(
            Diagnostic diagnostic)
        {
            var message = diagnostic.Descriptor.Description.ToString();
            return new RoslynTestKitException(message);
        }

        public static RoslynTestKitException DiagnosticNotFound(string diagnosticId, IDiagnosticLocator locator, Diagnostic[] reportedDiagnostics)
        {
            var reportedDiagnosticInfo = reportedDiagnostics.MergeWithComma(x => x.Id, title: "Reported issues: ");
            var message = $"There is no issue reported for {diagnosticId} at {locator.Description()}.{reportedDiagnosticInfo}";
            return new RoslynTestKitException(message);
        }

        public static RoslynTestKitException CodeFixNotFound(int expectedCodeFixIndex, ImmutableArray<CodeAction> codeFixes, IDiagnosticLocator locator)
        {
            var codeFixDescription = GetActionsDescription(codeFixes, " Found only {codeFixes.Length} CodeFixes: ");
            var message = $"Cannot find CodeFix with index {expectedCodeFixIndex} at {locator.Description()}.{codeFixDescription}";
            return new RoslynTestKitException(message);
        }

        public static RoslynTestKitException CodeRefactoringNotFound(int expectedCodeRefactoringIndex, ImmutableArray<CodeAction> codeRefactorings, IDiagnosticLocator locator)
        {
            var refactoringDescriptions = GetActionsDescription(codeRefactorings, $" Found only {codeRefactorings.Length} CodeRefactorings: ");
            var message = $"Cannot find CodeRefactoring with index {expectedCodeRefactoringIndex}  at {locator.Description()}.{refactoringDescriptions}";
            return new RoslynTestKitException(message);
        }

        public static RoslynTestKitException UnexpectedCodeRefactorings(ImmutableArray<CodeAction> codeRefactorings)
        {
            var refactoringDescriptions = GetActionsDescription(codeRefactorings);
            return new RoslynTestKitException($"Found reported CodeRefactorings '{refactoringDescriptions}' in spite of the expectations ");
        }

        private static string GetActionsDescription(ImmutableArray<CodeAction> codeFixes, string title = null)
        {
            if (codeFixes.Length == 0)
            {
                return string.Empty;
            }

            return title + string.Join(", ", codeFixes.Select((x, index) => $"[{index}] = {x.Title}"));
        }

        public static RoslynTestKitException NoOperationForCodeAction(CodeAction codeAction)
        {
            return new RoslynTestKitException($"There is no operation associated with '{codeAction.Title}'");
        }

        public static RoslynTestKitException MoreThanOneOperationForCodeAction(CodeAction codeAction, List<CodeActionOperation> operations)
        {
            var foundOperationDescriptions = operations.MergeWithComma(x => x.Title, title: " Found operations: ");
            return new RoslynTestKitException($"There is more than one operation associated with '{codeAction.Title}'.{foundOperationDescriptions}");
        }

        public static Exception CannotFindSuggestion(IReadOnlyList<string> missingCompletion, ImmutableArray<CompletionItem> resultItems, IDiagnosticLocator locator)
        {
            var foundSuggestionDescription = resultItems.MergeAsBulletList(x => x.DisplayText, title: "\r\nFound suggestions:\r\n");
            return new RoslynTestKitException($"Cannot get suggestions:\r\n{missingCompletion.MergeAsBulletList()}\r\nat{locator.Description()}{foundSuggestionDescription}");
        }
    }
}