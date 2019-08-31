using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using RoslynTestKit.Utils;

namespace RoslynTestKit
{
    public abstract class AnalyzerTestFixture : BaseTestFixture
    {
        protected virtual ImmutableList<MetadataReference> References => null;
        protected abstract DiagnosticAnalyzer CreateAnalyzer();

        protected void NoDiagnostic(string code, string diagnosticId)
        {
            var document = MarkupHelper.GetDocumentFromCode(code, LanguageName, References);
            NoDiagnostic(document, diagnosticId);
        }

        protected void NoDiagnostic(Document document, string diagnosticId)
        {
            var diagnostics = GetDiagnostics(document);
            var hasDiagnostic = diagnostics.Any(d => d.Id == diagnosticId);
            if (hasDiagnostic)
            {
                throw RoslynTestKitException.UnexpectedDiagnostic(diagnosticId);
            }
        }

        protected void HasDiagnostic(string markupCode, string diagnosticId)
        {
            var document = MarkupHelper.GetDocumentFromMarkup(markupCode, LanguageName, References);
            var locator = MarkupHelper.GetLocator(markupCode);
            HasDiagnostic(document, diagnosticId, locator);
        }

        protected void HasDiagnosticAtLine(string code, string diagnosticId, int lineNumber)
        {
            var document = MarkupHelper.GetDocumentFromCode(code, LanguageName, References);
            var locator = LineLocator.FromCode(code, lineNumber);
            HasDiagnostic(document, diagnosticId, locator);
        }

        protected void HasDiagnosticAtLine(Document document, string diagnosticId, int lineNumber)
        {
            var locator = LineLocator.FromDocument(document, lineNumber);
            HasDiagnostic(document, diagnosticId, locator);
        }

        protected void HasDiagnostic(Document document, string diagnosticId, TextSpan span)
        {
            var locator = new TextSpanLocator(span);
            HasDiagnostic(document, diagnosticId, locator);
        }

        private void HasDiagnostic(Document document, string diagnosticId, IDiagnosticLocator locator)
        {
            var reporteddiagnostics = GetDiagnostics(document).Where(d => locator.Match(d.Location)).ToArray();
            var matchedDiagnostics = reporteddiagnostics.Count(d => d.Id == diagnosticId);

            if (matchedDiagnostics == 0)
            {
                throw RoslynTestKitException.DiagnosticNotFound(diagnosticId, locator, reporteddiagnostics);
            }
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
