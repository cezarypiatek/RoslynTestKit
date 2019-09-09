using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using RoslynTestKit.Utils;

namespace RoslynTestKit
{
    public abstract class CodeFixTestFixture : BaseTestFixture
    {
        protected virtual ImmutableList<MetadataReference> References => null;
        protected abstract CodeFixProvider CreateProvider();

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
            TestCodeFix(document, expected, descriptor, locator, codeFixIndex);
        }
        protected void TestCodeFix(string markupCode, string expected, DiagnosticDescriptor descriptor, int codeFixIndex = 0)
        {
            var document = MarkupHelper.GetDocumentFromMarkup(markupCode, LanguageName, References);
            var locator = MarkupHelper.GetLocator(markupCode);
            TestCodeFix(document, expected, descriptor, locator, codeFixIndex);
        }

        protected void TestCodeFix(Document document, string expected, DiagnosticDescriptor descriptor, TextSpan span, int codeFixIndex = 0)
        {
           var locator = new TextSpanLocator(span);
           TestCodeFix(document, expected, descriptor, locator, codeFixIndex);
        }

        private void TestCodeFix(Document document, string expected, string diagnosticId, IDiagnosticLocator locator, int codeFixIndex)
        {
            var reportedDiagnostics = GetReportedDiagnostics(document, locator).ToArray();
            var diagnostic = reportedDiagnostics.FirstOrDefault(x => x.Id == diagnosticId);
            if (diagnostic == null)
            {
                throw RoslynTestKitException.DiagnosticNotFound(diagnosticId, locator, reportedDiagnostics);
            }
            TestCodeFix(document, expected, diagnostic.Descriptor, locator, codeFixIndex);
        }

        private void TestCodeFix(Document document, string expected, DiagnosticDescriptor descriptor, IDiagnosticLocator locator, int codeFixIndex = 0)
        {
            var codeFixes = GetCodeFixes(document, locator, descriptor);
            if (codeFixes.Length < codeFixIndex + 1)
            {
                throw RoslynTestKitException.CodeFixNotFound(codeFixIndex, codeFixes, locator);
            }
            Verify.CodeAction(codeFixes[codeFixIndex], document, expected);
        }

        private static IEnumerable<Diagnostic> GetReportedDiagnostics(Document document, IDiagnosticLocator locator)
        {
            return document.GetSemanticModelAsync().Result.GetDiagnostics().Where(d=> locator.Match(d.Location));
        }

        private ImmutableArray<CodeAction> GetCodeFixes(Document document, IDiagnosticLocator locator, DiagnosticDescriptor descriptor)
        {
            var builder = ImmutableArray.CreateBuilder<CodeAction>();
            var diagnostic = FindOrCreateDiagnosticForDescriptor(document, descriptor, locator);
            var context = new CodeFixContext(document, diagnostic, (a, _) => builder.Add(a), CancellationToken.None);

            var provider = CreateProvider();
            provider.RegisterCodeFixesAsync(context).Wait();

            return builder.ToImmutable();
        }

        private static Diagnostic FindOrCreateDiagnosticForDescriptor(Document document, DiagnosticDescriptor descriptor, IDiagnosticLocator locator)
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
