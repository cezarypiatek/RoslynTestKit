using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using RoslynTestKit.Utils;

namespace RoslynTestKit
{
    public abstract class CodeFixTestFixture : BaseTestFixture
    {
        protected abstract CodeFixProvider CreateProvider();

        protected virtual IReadOnlyCollection<DiagnosticAnalyzer> CreateAdditionalAnalyzers() => null;


        protected void NoCodeFix(string markupCode, string diagnosticId)
        {
            var document = MarkupHelper.GetDocumentFromMarkup(markupCode, LanguageName, References);
            var locator = MarkupHelper.GetLocator(markupCode);
            NoCodeFix(document, diagnosticId, locator);
        }

        protected void NoCodeFixAtLine(string code, string diagnosticId, int line)
        {
            var document = MarkupHelper.GetDocumentFromMarkup(code, LanguageName, References);
            var locator = LineLocator.FromCode(code, line);
            NoCodeFix(document, diagnosticId, locator);
        }

        protected void NoCodeFixAtLine(string code, DiagnosticDescriptor descriptor, int line)
        {
            var document = MarkupHelper.GetDocumentFromMarkup(code, LanguageName, References);
            var locator = LineLocator.FromCode(code, line);
            var diagnostic = FindOrCreateDiagnosticForDescriptor(document, descriptor, locator);
            NoCodeFix(document, diagnostic, locator);
        }
        protected void NoCodeFix(string markupCode, DiagnosticDescriptor descriptor)
        {
            var document = MarkupHelper.GetDocumentFromMarkup(markupCode, LanguageName, References);
            var locator = MarkupHelper.GetLocator(markupCode);
            var diagnostic = FindOrCreateDiagnosticForDescriptor(document, descriptor, locator);
            NoCodeFix(document, diagnostic, locator);
        }

        protected void NoCodeFix(Document document, DiagnosticDescriptor descriptor, TextSpan span)
        {
            var locator = new TextSpanLocator(span);
            var diagnostic = FindOrCreateDiagnosticForDescriptor(document, descriptor, locator);
            NoCodeFix(document, diagnostic, locator);
        }

        protected void TestCodeFix(string markupCode, string expected, string diagnosticId, int codeFixIndex = 0)
        {
            var document = MarkupHelper.GetDocumentFromMarkup(markupCode, LanguageName, References);
            var locator = MarkupHelper.GetLocator(markupCode);
            TestCodeFix(document, expected, diagnosticId, locator, codeFixIndex);
        }

        protected void TestCodeFixAtLine(string code, string expected, string diagnosticId, int line, int codeFixIndex = 0)
        {
            var document = MarkupHelper.GetDocumentFromMarkup(code, LanguageName, References);
            var locator = LineLocator.FromCode(code, line);
            TestCodeFix(document, expected, diagnosticId, locator, codeFixIndex);
        }

        protected void TestCodeFixAtLine(string code, string expected, DiagnosticDescriptor descriptor, int line, int codeFixIndex = 0)
        {
            var document = MarkupHelper.GetDocumentFromMarkup(code, LanguageName, References);
            var locator = LineLocator.FromCode(code, line);
            var diagnostic = FindOrCreateDiagnosticForDescriptor(document, descriptor, locator);
            TestCodeFix(document, expected, diagnostic, locator, codeFixIndex);
        }
        protected void TestCodeFix(string markupCode, string expected, DiagnosticDescriptor descriptor, int codeFixIndex = 0)
        {
            var document = MarkupHelper.GetDocumentFromMarkup(markupCode, LanguageName, References);
            var locator = MarkupHelper.GetLocator(markupCode);
            var diagnostic = FindOrCreateDiagnosticForDescriptor(document, descriptor, locator);
            TestCodeFix(document, expected, diagnostic, locator, codeFixIndex);
        }

        protected void TestCodeFix(Document document, string expected, DiagnosticDescriptor descriptor, TextSpan span, int codeFixIndex = 0)
        {
            var locator = new TextSpanLocator(span);
            var diagnostic = FindOrCreateDiagnosticForDescriptor(document, descriptor, locator);
            TestCodeFix(document, expected, diagnostic, locator, codeFixIndex);
        }

        private void TestCodeFix(Document document, string expected, string diagnosticId, IDiagnosticLocator locator, int codeFixIndex)
        {
            var diagnostic = GetDiagnostic(document, diagnosticId, locator);
            TestCodeFix(document, expected, diagnostic, locator, codeFixIndex);
        }

        private void NoCodeFix(Document document, string diagnosticId, IDiagnosticLocator locator)
        {
            var diagnostic = GetDiagnostic(document, diagnosticId, locator);
            NoCodeFix(document, diagnostic, locator);
        }

        private void NoCodeFix(Document document, Diagnostic diagnostic, IDiagnosticLocator locator)
        {
            var codeFixes = GetCodeFixes(document, diagnostic);
            if (codeFixes.Length != 0)
            {
                throw RoslynTestKitException.UnexpectedCodeFixFound(codeFixes, locator);
            }
        }

        private Diagnostic GetDiagnostic(Document document, string diagnosticId, IDiagnosticLocator locator)
        {
            var reportedDiagnostics = GetReportedDiagnostics(document, locator).ToArray();
            var diagnostic = reportedDiagnostics.FirstOrDefault(x => x.Id == diagnosticId);
            if (diagnostic == null)
            {
                throw RoslynTestKitException.DiagnosticNotFound(diagnosticId, locator, reportedDiagnostics);
            }

            return diagnostic;
        }

        private void TestCodeFix(Document document, string expected, Diagnostic diagnostic, IDiagnosticLocator locator, int codeFixIndex = 0)
        {
            var codeFixes = GetCodeFixes(document, diagnostic);
            if (codeFixes.Length < codeFixIndex + 1)
            {
                throw RoslynTestKitException.CodeFixNotFound(codeFixIndex, codeFixes, locator);
            }
            Verify.CodeAction(codeFixes[codeFixIndex], document, expected);
        }

        private IEnumerable<Diagnostic> GetReportedDiagnostics(Document document, IDiagnosticLocator locator)
        {
            return GetAllReportedDiagnostics(document).Where(d => locator.Match(d.Location));
        }

        private IEnumerable<Diagnostic> GetAllReportedDiagnostics(Document document)
        {
            var additionalAnalyzers = CreateAdditionalAnalyzers();
            if (additionalAnalyzers != null)
            {
                var documentTree = document.GetSyntaxTreeAsync().GetAwaiter().GetResult();

                return document.Project.GetCompilationAsync().GetAwaiter().GetResult()
                    .WithAnalyzers(additionalAnalyzers.ToImmutableArray())
                    .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult()
                    .Where(x=>x.Location.SourceTree == documentTree);
            }

            return document.GetSemanticModelAsync().GetAwaiter().GetResult().GetDiagnostics();
        }

        private ImmutableArray<CodeAction> GetCodeFixes(Document document, Diagnostic diagnostic)
        {
            var builder = ImmutableArray.CreateBuilder<CodeAction>();
            var context = new CodeFixContext(document, diagnostic, (a, _) => builder.Add(a), CancellationToken.None);
            var provider = CreateProvider();
            provider.RegisterCodeFixesAsync(context).GetAwaiter().GetResult();
            return builder.ToImmutable();
        }

        private Diagnostic FindOrCreateDiagnosticForDescriptor(Document document, DiagnosticDescriptor descriptor, IDiagnosticLocator locator)
        {
            var reportedDiagnostics = GetReportedDiagnostics(document, locator).ToList();
            var diagnostic = reportedDiagnostics.FirstOrDefault(x => x.Id == descriptor.Id);
            if (diagnostic != null)
            {
                return diagnostic;
            }

            var tree = document.GetSyntaxTreeAsync(CancellationToken.None).Result;
            return Diagnostic.Create(descriptor, Location.Create(tree, locator.GetSpan()));
        }
    }
}
