using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;

namespace RoslynTestKit
{
    internal class ConfigurableCodeRefactoringTestFixture: CodeRefactoringTestFixture
    {
        private readonly CodeRefactoringTestFixtureConfig _config;
        private CodeRefactoringProvider _provider;

        public ConfigurableCodeRefactoringTestFixture(CodeRefactoringProvider provider, CodeRefactoringTestFixtureConfig config)
        {
            _config = config;
            _provider = provider;
        }

        protected override string LanguageName => _config.Language;
        protected override CodeRefactoringProvider CreateProvider() => _provider;
        protected override IReadOnlyCollection<MetadataReference> References => _config.References;
        protected override IReadOnlyCollection<AdditionalText> AdditionalFiles => _config.AdditionalFiles;
        protected override bool ThrowsWhenInputDocumentContainsError => _config.ThrowsWhenInputDocumentContainsError;
    }
}