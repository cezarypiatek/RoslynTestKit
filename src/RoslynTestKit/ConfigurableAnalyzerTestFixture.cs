using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RoslynTestKit
{
    internal class ConfigurableAnalyzerTestFixture: AnalyzerTestFixture
    {
        private readonly DiagnosticAnalyzer _analyzer;
        private readonly AnalyzerTestFixtureConfig _config;

        public ConfigurableAnalyzerTestFixture(DiagnosticAnalyzer diagnosticAnalyzer, AnalyzerTestFixtureConfig config)
        {
            _analyzer = diagnosticAnalyzer;
            _config = config;
        }

        protected override string LanguageName => _config.Language;
        protected override DiagnosticAnalyzer CreateAnalyzer() => _analyzer;
        protected override IReadOnlyCollection<MetadataReference> References => _config.References;
        protected override IReadOnlyCollection<AdditionalText> AdditionalFiles => _config.AdditionalFiles;
        protected override bool ThrowsWhenInputDocumentContainsError => _config.ThrowsWhenInputDocumentContainsError;
    }
}