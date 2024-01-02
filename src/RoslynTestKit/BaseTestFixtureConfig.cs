using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace RoslynTestKit
{
    public abstract class BaseTestFixtureConfig
    {
        public IReadOnlyList<MetadataReference> References { get; set; } = ImmutableArray<MetadataReference>.Empty;
        
        public bool ThrowsWhenInputDocumentContainsError { get; set;} = true;
        public string Language { get; set;} = LanguageNames.CSharp;
        
        public IReadOnlyList<AdditionalText> AdditionalFiles { get; set;} = ImmutableArray<AdditionalText>.Empty;
    }
}