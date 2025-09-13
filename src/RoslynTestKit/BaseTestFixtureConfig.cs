using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using AdditionalText = Microsoft.Dynamics.Nav.CodeAnalysis.AdditionalText;
using LanguageNames = Microsoft.Dynamics.Nav.CodeAnalysis.LanguageNames;

namespace RoslynTestKit
{
    public abstract class BaseTestFixtureConfig
    {
        public IReadOnlyList<MetadataReference> References { get; set; } = ImmutableArray<MetadataReference>.Empty;

        public bool ThrowsWhenInputDocumentContainsError { get; set; } = true;
        public string Language { get; set; } = LanguageNames.AL;

        public IReadOnlyList<AdditionalText> AdditionalFiles { get; set; } = ImmutableArray<AdditionalText>.Empty;
    }
}