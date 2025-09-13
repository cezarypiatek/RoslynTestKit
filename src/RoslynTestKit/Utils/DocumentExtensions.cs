using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace RoslynTestKit.Utils
{
    static class DocumentExtensions
    {
        public static IReadOnlyList<Diagnostic> GetErrors(this Document document)
        {
            var diagnostics = document.GetSemanticModelAsync().GetAwaiter().GetResult().GetDiagnostics();
            return diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        }
    }
}
