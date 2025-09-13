using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace RoslynTestKit.Utils
{
    static class DocumentExtensions
    {
        public static IReadOnlyList<Diagnostic> GetErrors(this Document document)
        {
            var semanticModel = document.GetSemanticModelAsync().GetAwaiter().GetResult();
            if (semanticModel == null)
            {
                return new List<Diagnostic>();
            }

            var diagnostics = semanticModel.GetDiagnostics();
            return diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        }
    }
}