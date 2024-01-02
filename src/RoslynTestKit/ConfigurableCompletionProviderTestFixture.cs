using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;

namespace RoslynTestKit
{
    internal class ConfigurableCompletionProviderTestFixture: CompletionProviderFixture 
    {
        private readonly CompletionProviderTestFixtureConfig _config;
        private CompletionProvider _provider;

        public ConfigurableCompletionProviderTestFixture(CompletionProvider provider, CompletionProviderTestFixtureConfig config)
        {
            _config = config;
            _provider = provider;
        }

        protected override string LanguageName => _config.Language;
        protected override CompletionProvider CreateProvider() => _provider;
        protected override IReadOnlyCollection<MetadataReference> References => _config.References;
        protected override IReadOnlyCollection<AdditionalText> AdditionalFiles => _config.AdditionalFiles;
        protected override bool ThrowsWhenInputDocumentContainsError => _config.ThrowsWhenInputDocumentContainsError;
    }
}