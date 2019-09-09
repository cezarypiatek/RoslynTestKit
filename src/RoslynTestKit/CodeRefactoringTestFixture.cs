using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;
using RoslynTestKit.Utils;

namespace RoslynTestKit
{
    public abstract class CodeRefactoringTestFixture : BaseTestFixture
    {
        protected virtual ImmutableList<MetadataReference> References => null;
        protected abstract CodeRefactoringProvider CreateProvider();

        protected void TestCodeRefactoring(string markupCode, string expected, int refactoringIndex=0)
        {
            var document = MarkupHelper.GetDocumentFromMarkup(markupCode, LanguageName, References);
            var locator = MarkupHelper.GetLocator(markupCode);
            TestCodeRefactoring(document, expected, locator, refactoringIndex);
        }

        protected void TestCodeRefactoringAtLine(string code, string expected, int line, int refactoringIndex=0)
        {
            var document = MarkupHelper.GetDocumentFromCode(code, LanguageName, References);
            var locator = LineLocator.FromCode(code, line);
            TestCodeRefactoring(document, expected, locator, refactoringIndex);
        }
        protected void TestCodeRefactoringAtLine(Document document, string expected, int line, int refactoringIndex=0)
        {
            var locator = LineLocator.FromDocument(document, line);
            TestCodeRefactoring(document, expected, locator, refactoringIndex);
        }

        protected void TestCodeRefactoring(Document document, string expected, TextSpan span, int refactoringIndex = 0)
        {
            var locator = new TextSpanLocator(span);
            TestCodeRefactoring(document, expected, locator, refactoringIndex);
        }

        private void TestCodeRefactoring(Document document, string expected, IDiagnosticLocator locator, int refactoringIndex = 0)
        {
            var codeRefactorings = GetCodeRefactorings(document, locator);
            if (codeRefactorings.Length < refactoringIndex + 1)
            {
                throw RoslynTestKitException.CodeRefactoringNotFound(refactoringIndex, codeRefactorings, locator);
            }
            Verify.CodeAction(codeRefactorings[refactoringIndex], document, expected);
        }

        protected void TestNoCodeRefactoring(string markupCode)
        {
            var document = MarkupHelper.GetDocumentFromMarkup(markupCode, LanguageName, References);
            var locator = MarkupHelper.GetLocator(markupCode);
            TestNoCodeRefactoring(document, locator);
        }

        protected void TestNoCodeRefactoring(Document document, TextSpan span)
        {
            var locator = new TextSpanLocator(span);
            TestNoCodeRefactoring(document, locator);
        }

        private void TestNoCodeRefactoring(Document document, IDiagnosticLocator locator)
        {
            var codeRefactorings = GetCodeRefactorings(document, locator).ToImmutableArray();
            if (codeRefactorings.Length != 0)
            {
                throw RoslynTestKitException.UnexpectedCodeRefactorings(codeRefactorings);
            }
        }

        private ImmutableArray<CodeAction> GetCodeRefactorings(Document document, IDiagnosticLocator locator)
        {
            var builder = ImmutableArray.CreateBuilder<CodeAction>();
            var context = new CodeRefactoringContext(document, locator.GetSpan(), a => builder.Add(a), CancellationToken.None);
            var provider = CreateProvider();
            provider.ComputeRefactoringsAsync(context).Wait();
            return builder.ToImmutable();
        }
    }
}
