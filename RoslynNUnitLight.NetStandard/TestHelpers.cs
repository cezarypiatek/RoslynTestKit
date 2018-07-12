using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using RoslynNUnitLight.Reflection;

namespace RoslynNUnitLight
{
    public static class TestHelpers
    {
        public static bool TryGetCodeAndSpanFromMarkup(string markupCode, out string code, out TextSpan span)
        {
            code = null;
            span = default(TextSpan);

            var builder = new StringBuilder();

            var start = markupCode.IndexOf("[|");
            if (start < 0)
            {
                return false;
            }

            builder.Append(markupCode.Substring(0, start));

            var end = markupCode.IndexOf("|]");
            if (end < 0)
            {
                return false;
            }

            builder.Append(markupCode.Substring(start + 2, end - start - 2));
            builder.Append(markupCode.Substring(end + 2));

            code = builder.ToString();
            span = TextSpan.FromBounds(start, end - 2);

            return true;
        }

        public static bool TryGetDocumentAndSpanFromMarkup(string markupCode, string languageName, out Document document, out TextSpan span)
        {
            return TryGetDocumentAndSpanFromMarkup(markupCode, languageName, null, out document, out span);
        }

        public static bool TryGetDocumentAndSpanFromMarkup(string markupCode, string languageName, ImmutableList<MetadataReference> references, out Document document, out TextSpan span)
        {
            string code;
            if (!TryGetCodeAndSpanFromMarkup(markupCode, out code, out span))
            {
                document = null;
                return false;
            }

            document = GetDocument(code, languageName, references);
            return true;
        }

        public static Document GetDocument(string code, string languageName, ImmutableList<MetadataReference> references = null)
        {
            references = references ?? ImmutableList.Create<MetadataReference>(
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.GetLocation()),
                MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.GetLocation()));

            return new AdhocWorkspace()
                .AddProject("TestProject", languageName)
                .AddMetadataReferences(references)
                .AddDocument("TestDocument", code);
        }
    }
}
