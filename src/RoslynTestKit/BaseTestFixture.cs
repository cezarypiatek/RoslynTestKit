using Microsoft.CodeAnalysis;

namespace RoslynTestKit
{
    public abstract class BaseTestFixture
    {
        protected abstract string LanguageName { get; }

        protected virtual MetadataReference[] References => null;
    }
}
