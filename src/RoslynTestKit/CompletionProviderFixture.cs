using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using RoslynTestKit.Utils;

namespace RoslynTestKit
{
    public abstract class CompletionProviderFixture : BaseTestFixture
    {
        protected void TestCompletion(string markupCode, string[] expectedCompletions, CompletionTrigger? trigger=null)
        {
            var markup = new CodeMarkup(markupCode);
            var document = CreateDocumentFromCode(markup.Code);
            var assertion = CreateAssertionBasedOnExpectedSet(expectedCompletions, markup.Locator);
            VerifyExpectations(document, markup.Locator, trigger, assertion);
        }

        protected void TestCompletion(string markupCode, Action<ImmutableArray<CompletionItem>> assertion, CompletionTrigger? trigger=null)
        {
            var markup = new CodeMarkup(markupCode);
            var document = CreateDocumentFromCode(markup.Code);
            VerifyExpectations(document, markup.Locator, trigger, assertion);
        }

        protected void TestCompletion(Document document, TextSpan span, string[] expectedCompletions, CompletionTrigger? trigger = null)
        {
            var locator = new TextSpanLocator(span);
            var assertion = CreateAssertionBasedOnExpectedSet(expectedCompletions, locator);
            VerifyExpectations(document, locator, trigger, assertion);
        }

        protected void TestCompletion(Document document, TextSpan span, Action<ImmutableArray<CompletionItem>> assertion, CompletionTrigger? trigger = null)
        {
            var locator = new TextSpanLocator(span);
            VerifyExpectations(document, locator, trigger, assertion);
        }

        private static Action<ImmutableArray<CompletionItem>> CreateAssertionBasedOnExpectedSet(string[] expectedCompletions, IDiagnosticLocator locator)
        {
            return (items) =>
            {
                var allFoundCompletionText = items.Select(x => x.DisplayText);
                var missingSuggestions = expectedCompletions.Except(allFoundCompletionText).ToList();

                if (missingSuggestions.Count > 0)
                {
                    throw RoslynTestKitException.CannotFindSuggestion(missingSuggestions, items, locator);
                }
            };
        }

        private void VerifyExpectations(Document document, IDiagnosticLocator locator, CompletionTrigger? trigger, Action<ImmutableArray<CompletionItem>> assertion)
        {
            var selectedTrigger = trigger ?? CompletionTrigger.Invoke;
            var provider = CreateProvider();
            var span = locator.GetSpan();
            var options = document.GetOptionsAsync(CancellationToken.None).GetAwaiter().GetResult();
            var service = new TestCompletionService(document.Project.Solution.Workspace, LanguageName, provider);
            var result = service.GetCompletionsAsync(document, span.Start, selectedTrigger, ImmutableHashSet<string>.Empty, options, CancellationToken.None).GetAwaiter().GetResult();
            assertion(result?.Items ?? ImmutableArray<CompletionItem>.Empty);
        }

        protected abstract CompletionProvider CreateProvider();
    }
}