using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RoslynTestKit.Utils
{
    internal static class MarkupHelper
    {
        public static Document GetDocumentFromMarkup(string markup, string languageName, IReadOnlyCollection<MetadataReference> references, string projectName = null, string documentName = null)
        {
            var code = markup.Replace("[|", "").Replace("|]", "");
            return GetDocumentFromCode(code, languageName, references, projectName, documentName);
        }

        public static Document GetDocumentFromCode(string code, string languageName, IReadOnlyCollection<MetadataReference> references, string projectName= null, string documentName= null)
        {
            var immutableReferencesBuilder = ImmutableArray.CreateBuilder<MetadataReference>();
            if (references != null)
            {
                immutableReferencesBuilder.AddRange(references);
            }
            immutableReferencesBuilder.Add(ReferenceSource.Core);
            immutableReferencesBuilder.Add(ReferenceSource.Linq);

            return new AdhocWorkspace()
                .AddProject(projectName ?? "TestProject", languageName)
                .AddMetadataReferences(immutableReferencesBuilder.ToImmutable())
                .AddDocument(documentName ?? "TestDocument", code);
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