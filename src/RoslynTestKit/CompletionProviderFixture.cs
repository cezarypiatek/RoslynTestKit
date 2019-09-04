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
        protected virtual ImmutableList<MetadataReference> References => null;

        protected void TestCompletion(string markupCode, string[] expectedCompletions, CompletionTrigger? trigger=null)
        {
            var document = MarkupHelper.GetDocumentFromMarkup(markupCode, LanguageName, References);
            var locator = MarkupHelper.GetLocator(markupCode);
            var assertion = CreateAssertionBasedOnExpectedSet(expectedCompletions);
            VerifyExpectations(document, locator, trigger, assertion);
        }

        protected void TestCompletion(string markupCode, Action<ImmutableArray<CompletionItem>> assertion, CompletionTrigger? trigger=null)
        {
            var document = MarkupHelper.GetDocumentFromMarkup(markupCode, LanguageName, References);
            var locator = MarkupHelper.GetLocator(markupCode);
            VerifyExpectations(document, locator, trigger, assertion);
        }

        protected void TestCompletion(Document document, TextSpan span, string[] expectedCompletions, CompletionTrigger? trigger = null)
        {
            var locator = new TextSpanLocator(span);
            var assertion = CreateAssertionBasedOnExpectedSet(expectedCompletions);
            VerifyExpectations(document, locator, trigger, assertion);
        }

        protected void TestCompletion(Document document, TextSpan span, Action<ImmutableArray<CompletionItem>> assertion, CompletionTrigger? trigger = null)
        {
            var locator = new TextSpanLocator(span);
            VerifyExpectations(document, locator, trigger, assertion);
        }

        private static Action<ImmutableArray<CompletionItem>> CreateAssertionBasedOnExpectedSet(string[] expectedCompletions)
        {
            return (items) =>
            {
                var allFoundCompletionText = items.Select(x => x.DisplayText);
                var missingSuggestions = expectedCompletions.Except(allFoundCompletionText).ToList();

                if (missingSuggestions.Count > 0)
                {
                    throw RoslynTestKitException.CannotFindSuggestion(missingSuggestions, items);
                }
            };
        }

        private void VerifyExpectations(Document document, IDiagnosticLocator locator, CompletionTrigger? trigger, Action<ImmutableArray<CompletionItem>> assertion)
        {
            var selectedTrigger = trigger ?? CompletionTrigger.Default;
            var provider = CreateProvider();
            var span = locator.GetSpan();
            var options = document.GetOptionsAsync(CancellationToken.None).GetAwaiter().GetResult();
            var service = new TestCompletionService(document.Project.Solution.Workspace, LanguageName, provider);
            var result = service.GetCompletionsAsync(document, span.Start, selectedTrigger, ImmutableHashSet<string>.Empty, options, CancellationToken.None).GetAwaiter().GetResult();
            assertion(result.Items);
        }

        protected abstract CompletionProvider CreateProvider();
    }
}