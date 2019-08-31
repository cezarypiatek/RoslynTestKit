using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RoslynNUnitLight
{
    internal static class MarkupHelper
    {
        public static Document GetDocumentFromMarkup(string markup, string languageName, ImmutableList<MetadataReference> references = null)
        {
            var code = markup.Replace("[|", "").Replace("|]", "");
            return GetDocumentFromCode(code, languageName, references);
        }

        public static Document GetDocumentFromCode(string code, string languageName, ImmutableList<MetadataReference> references)
        {
            references = references ?? ImmutableList<MetadataReference>.Empty;
            references = references
                .Add(References.Core)
                .Add(References.Linq);

            return new AdhocWorkspace()
                .AddProject("TestProject", languageName)
                .AddMetadataReferences(references)
                .AddDocument("TestDocument", code);
        }

        public static IDiagnosticLocator GetLocator(string markupCode)
        {
            if (TryFindMarkedTimeSpan(markupCode, out var textSpan))
            {
                return new TextSpanLocator(textSpan);
            }

            throw new Exception("Cannot find any position marked with [||]");
        }

        private static bool TryFindMarkedTimeSpan(string markupCode, out TextSpan textSpan)
        {
            textSpan = default;
            var start = markupCode.IndexOf("[|", StringComparison.InvariantCulture);
            if (start < 0)
            {
                return false;
            }

            var end = markupCode.IndexOf("|]", StringComparison.InvariantCulture);
            if (end < 0)
            {
                return false;
            }

            textSpan = TextSpan.FromBounds(start, end - 2);
            return true;
        }
    }
}