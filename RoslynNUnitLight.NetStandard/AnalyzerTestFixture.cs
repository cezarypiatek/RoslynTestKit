using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace RoslynNUnitLight
{
    public abstract class AnalyzerTestFixture : BaseTestFixture
    {
        protected virtual ImmutableList<MetadataReference> References => null;
        protected abstract DiagnosticAnalyzer CreateAnalyzer();

        protected void NoDiagnostic(string code, string diagnosticId)
        {
            var document = TestHelpers.GetDocument(code, LanguageName);

            NoDiagnostic(document, diagnosticId);
        }

        protected void NoDiagnostic(Document document, string diagnosticId)
        {
            var diagnostics = GetDiagnostics(document);
            Assert.That(diagnostics.Any(d => d.Id == diagnosticId), Is.False);
        }

        protected void HasDiagnostic(string markupCode, string diagnosticId)
        {
            var document = MarkupHelper.GetDocumentFromMarkup(markupCode, LanguageName, References);
            var locator = MarkupHelper.GetLocator(markupCode);
            HasDiagnostic(document, diagnosticId, locator);
        }

        protected void HasDiagnostic(string code, string diagnosticId, int lineNumber)
        {
            var document = MarkupHelper.GetDocumentFromCode(code, LanguageName, References);
            var locator = LineLocator.FromCode(code, lineNumber);
            HasDiagnostic(document, diagnosticId, locator);
        }

        protected void HasDiagnostic(Document document, string diagnosticId, TextSpan span)
        {
            var locator = new TextSpanLocator(span);
            HasDiagnostic(document, diagnosticId, locator);
        }
        protected void HasDiagnostic(Document document, string diagnosticId, int lineNumber)
        {
            var locator = LineLocator.FromDocument(document, lineNumber);
            HasDiagnostic(document, diagnosticId, locator);
        }

        protected void HasDiagnostic(Document document, string diagnosticId, IDiagnosticLocator locator)
        {
            var matchedDiagnostics = GetDiagnostics(document)
                .Where(d => locator.Match(d.Location))
                .Count(d => d.Id == diagnosticId);

            Assert.That(matchedDiagnostics, Is.EqualTo(1));
        }

        private ImmutableArray<Diagnostic> GetDiagnostics(Document document)
        {
            var analyzers = ImmutableArray.Create(CreateAnalyzer());
            var compilation = document.Project.GetCompilationAsync(CancellationToken.None).Result;
            var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers, cancellationToken: CancellationToken.None);
            var discarded = compilation.GetDiagnostics(CancellationToken.None);

            var tree = document.GetSyntaxTreeAsync(CancellationToken.None).Result;

            var builder = ImmutableArray.CreateBuilder<Diagnostic>();
            foreach (var diagnostic in compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result)
            {
                var location = diagnostic.Location;
                if (location.IsInSource && location.SourceTree == tree)
                {
                    builder.Add(diagnostic);
                }
            }

            return builder.ToImmutable();
        }
    }
}
