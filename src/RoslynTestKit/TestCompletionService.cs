using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

namespace RoslynTestKit
{
    internal class TestCompletionService : CompletionServiceWithProviders
    {
        public TestCompletionService(Workspace workspace, string language, CompletionProvider provider)
            : base(workspace)
        {
            // Language = language;
            this.TestProviders = new[] { provider }.ToImmutableArray();
        }

        public ImmutableArray<CompletionProvider> TestProviders { get; }

        protected override ImmutableArray<CompletionProvider> GetProviders(ImmutableHashSet<string> roles, CompletionTrigger trigger)
        {
            return TestProviders;
        }

        // Microsoft.Dynamics.Nav.CodeAnalysis does not have LanguageNames
        // public override string Language { get; }
    }
}