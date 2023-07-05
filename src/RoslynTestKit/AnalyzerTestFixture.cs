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
        protected abstract DiagnosticAnalyzer CreateAnalyzer();

        protected void NoException(string code)
        {
            var document = CreateDocumentFromCode(code);
            var diagnostics = GetDiagnostics(document);
            var diagnostic = diagnostics.FirstOrDefault(d => d.Id == "AD0001");
            if (diagnostic != null)
            {
                throw RoslynTestKitException.ExceptionInAnalyzer(diagnostic);
            }
        }

        protected void NoDiagnostic(string code, string diagnosticId)
        {
            var document = CreateDocumentFromCode(code);
            NoDiagnostic(document, diagnosticId);
        }
        
        protected void NoDiagnostic(string code, string[] diagnosticIds)
        {
            var document = CreateDocumentFromCode(code);
            NoDiagnostic(document, diagnosticIds);
        }

        protected void NoDiagnosticAtLine(string code, string diagnosticId, int lineNumber)
        {
            var document = CreateDocumentFromCode(code);
            var locator = LineLocator.FromCode(code, lineNumber);
            NoDiagnostic(document, diagnosticId, locator);
        }
        
        protected void NoDiagnosticAtMarker(string markup, string diagnosticId)
        {
            var codeMarkup = new CodeMarkup(markup);
            var document = CreateDocumentFromCode(codeMarkup.Code);
            NoDiagnostic(document, diagnosticId, codeMarkup.Locator);
        }

        protected void NoDiagnostic(Document document, string diagnosticId, IDiagnosticLocator locator = null)
        {
            NoDiagnostic(document, new []{diagnosticId}, locator);
        }
        
        protected void NoDiagnostic(Document document, string[] diagnosticIds, IDiagnosticLocator locator = null)
        {
            var diagnostics = GetDiagnostics(document);
            if (locator != null)
            {
                diagnostics = diagnostics.Where(x => locator.Match(x.Location)).ToImmutableArray();
            }
            var unexpectedDiagnostics = diagnostics.Where(d => diagnosticIds.Contains(d.Id)).ToList();
            if (unexpectedDiagnostics.Count > 0)
            {
                throw RoslynTestKitException.UnexpectedDiagnostic(unexpectedDiagnostics);
            }
        }

        protected void HasDiagnostic(string markupCode, string diagnosticId)
        {
            var markup = new CodeMarkup(markupCode);
            var document = CreateDocumentFromCode(markup.Code);
            HasDiagnostic(document, diagnosticId, markup.Locator);
        }

        protected void HasDiagnosticAtLine(string code, string diagnosticId, int lineNumber)
        {
            var document = CreateDocumentFromCode(code);
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
            var errorsInDocument = discarded.Where(x => x.Severity == DiagnosticSeverity.Error).ToArray();
            if (errorsInDocument.Length > 0 && ThrowsWhenInputDocumentContainsError)
            {
                throw RoslynTestKitException.UnexpectedErrorDiagnostic(errorsInDocument);
            }

            var tree = document.GetSyntaxTreeAsync(CancellationToken.None).GetAwaiter().GetResult();

            var builder = ImmutableArray.CreateBuilder<Diagnostic>();
            foreach (var diagnostic in compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult())
            {
                builder.Add(diagnostic);
            }

            return builder.ToImmutable();
        }
    }
}
