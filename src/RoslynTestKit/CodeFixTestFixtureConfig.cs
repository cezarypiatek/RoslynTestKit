using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace RoslynTestKit
{
    public class CodeFixTestFixtureConfig : BaseTestFixtureConfig
    {
        public IReadOnlyCollection<DiagnosticAnalyzer> AdditionalAnalyzers { get; set; } = ImmutableArray<DiagnosticAnalyzer>.Empty;
    }
}