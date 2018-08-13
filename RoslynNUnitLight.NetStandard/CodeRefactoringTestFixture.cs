using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace RoslynNUnitLight
{
    public abstract class CodeRefactoringTestFixture : BaseTestFixture
    {
        protected virtual ImmutableList<MetadataReference> References => null;
        
        protected abstract CodeRefactoringProvider CreateProvider();

        protected void TestCodeRefactoring(string markupCode, string expected, int refactoringIndex=0)
        {
            bool documentCreated = TestHelpers.TryGetDocumentAndSpanFromMarkup(markupCode, LanguageName, References,
                out Document document, out TextSpan span);
            Assert.That(documentCreated, Is.True);

            TestCodeRefactoring(document, span, expected, refactoringIndex);
        }

        protected void TestCodeRefactoring(Document document, TextSpan span, string expected, int refactoringIndex=0)
        {
            var codeRefactorings = GetCodeRefactorings(document, span);

            Assert.That(codeRefactorings.Length, Is.AtLeast(refactoringIndex+1));

            Verify.CodeAction(codeRefactorings[refactoringIndex], document, expected);
        }

        protected void TestNoCodeRefactoring(string markupCode)
        {
            bool documentCreated = TestHelpers.TryGetDocumentAndSpanFromMarkup(markupCode, LanguageName, References,
                out Document document, out TextSpan span);
            Assert.That(documentCreated, Is.True);
            
            TestNoCodeRefactoring(document, span);
        }

        protected void TestNoCodeRefactoring(Document document, TextSpan span)
        {
            var codeRefactorings = GetCodeRefactorings(document, span);
            
            Assert.That(codeRefactorings, Is.Empty);
        }

        private ImmutableArray<CodeAction> GetCodeRefactorings(Document document, TextSpan span)
        {
            var builder = ImmutableArray.CreateBuilder<CodeAction>();
            Action<CodeAction> registerRefactoring = a => builder.Add(a);

            var context = new CodeRefactoringContext(document, span, registerRefactoring, CancellationToken.None);
            var provider = CreateProvider();
            provider.ComputeRefactoringsAsync(context).Wait();

            return builder.ToImmutable();
        }
    }
}
