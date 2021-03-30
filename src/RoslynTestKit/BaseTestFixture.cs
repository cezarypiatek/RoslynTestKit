using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using RoslynTestKit.Utils;

namespace RoslynTestKit
{
    public abstract class BaseTestFixture
    {
        protected abstract string LanguageName { get; }

        protected virtual bool ThrowsWhenInputDocumentContainsError { get; } = false;

        protected virtual IReadOnlyCollection<MetadataReference> References => null;

        protected Document CreateDocumentFromMarkup(string markup, string projectName = null, string documentName = null)
        {
            return MarkupHelper.GetDocumentFromMarkup(markup, LanguageName, References, projectName, documentName);
        }
        protected Document CreateDocumentFromCode(string code, string projectName = null, string documentName = null)
        {
            return MarkupHelper.GetDocumentFromCode(code, LanguageName, References, projectName, documentName);
        }

        protected IDiagnosticLocator GetMarkerLocation(string markupCode)
        {
            return MarkupHelper.GetLocator(markupCode);
        }
    }
}
