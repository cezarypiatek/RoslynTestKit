using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RoslynTestKit
{
    internal class ConfigurableCodeFixTestFixture : CodeFixTestFixture
    {
        private readonly CodeFixProvider _provider;
        private readonly CodeFixTestFixtureConfig _config;

        public ConfigurableCodeFixTestFixture(CodeFixProvider provider, CodeFixTestFixtureConfig config)
        {
            _provider = provider;
            _config = config;
        }

        protected override string LanguageName => _config.Language;
        protected override CodeFixProvider CreateProvider() => _provider;
        protected override IReadOnlyCollection<DiagnosticAnalyzer> CreateAdditionalAnalyzers() => _config.AdditionalAnalyzers;
        protected override IReadOnlyCollection<MetadataReference> References => _config.References;
        protected override IReadOnlyCollection<AdditionalText> AdditionalFiles => _config.AdditionalFiles;
        protected override bool ThrowsWhenInputDocumentContainsError => _config.ThrowsWhenInputDocumentContainsError;
    }
}