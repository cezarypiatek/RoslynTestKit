using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace RoslynNUnitLight
{
    public abstract class CodeFixTestFixture : BaseTestFixture
    {
        protected abstract CodeFixProvider CreateProvider();

        protected void TestCodeFix(string markupCode, string expected, string diagnosticId, int codeFixIndex = 0)
        {
            var (document, span) = GetDocumentAndSpanFromMarkup(markupCode);
            var reportedDiagnostics = GetReportedDiagnostics(document).ToList();
            var diagnostic = reportedDiagnostics.FirstOrDefault(x => x.Id == diagnosticId);
            Assert.That(diagnostic, Is.Not.Null, ()=>
            {
                var reportedIssues = reportedDiagnostics.Select(x => x.Id).ToList();
                return $"There is no issue reported for {diagnosticId}. Reported issues: {string.Join(",", reportedIssues)}";
            });
            TestCodeFix(document, span, expected, diagnostic.Descriptor, codeFixIndex);
        }

        protected void TestCodeFix(string markupCode, string expected, DiagnosticDescriptor descriptor, int codeFixIndex = 0)
        {
            var (document, span) = GetDocumentAndSpanFromMarkup(markupCode);
            TestCodeFix(document, span, expected, descriptor, codeFixIndex);
        }

        protected void TestCodeFix(Document document, TextSpan span, string expected, DiagnosticDescriptor descriptor, int codeFixIndex = 0)
        {
            var codeFixes = GetCodeFixes(document, span, descriptor);
            Assert.That(codeFixes.Length, Is.AtLeast(codeFixIndex + 1));
            Verify.CodeAction(codeFixes[codeFixIndex], document, expected);
        }

        private (Document document, TextSpan span) GetDocumentAndSpanFromMarkup(string markupCode)
        {
            Assert.That(TestHelpers.TryGetDocumentAndSpanFromMarkup(markupCode, LanguageName, out var document, out var span), Is.True);
            return (document, span);
        }


        private static IEnumerable<Diagnostic> GetReportedDiagnostics(Document document)
        {
            return document.GetSemanticModelAsync().Result.GetDiagnostics();
        }


        private ImmutableArray<CodeAction> GetCodeFixes(Document document, TextSpan span, DiagnosticDescriptor descriptor)
        {
            var builder = ImmutableArray.CreateBuilder<CodeAction>();
            var diagnostic = GetDiagnosticForDescriptor(document, span, descriptor);
            var context = new CodeFixContext(document, diagnostic, (a, _) => builder.Add(a), CancellationToken.None);

            var provider = CreateProvider();
            provider.RegisterCodeFixesAsync(context).Wait();

            return builder.ToImmutable();
        }

        private static Diagnostic GetDiagnosticForDescriptor(Document document, TextSpan span, DiagnosticDescriptor descriptor)
        {
            var reportedDiagnostics = GetReportedDiagnostics(document).ToList();
            var diagnostic = reportedDiagnostics.FirstOrDefault(x => x.Id == descriptor.Id);
            if (diagnostic != null)
            {
                return diagnostic;
            }

            var tree = document.GetSyntaxTreeAsync(CancellationToken.None).Result;
            return Diagnostic.Create(descriptor, Location.Create(tree, span));
        }
    }
}
