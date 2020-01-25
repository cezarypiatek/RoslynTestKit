using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;

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
            var metadataReferences = CreateMetadataReferences(references);

            var compilationOptions = GetCompilationOptions(languageName);

            return new AdhocWorkspace()
                .AddProject(projectName ?? "TestProject", languageName)
                .WithCompilationOptions(compilationOptions)
                .AddMetadataReferences(metadataReferences)
                .AddDocument(documentName ?? "TestDocument", code);
        }

        private static CompilationOptions GetCompilationOptions(string languageName) =>
            languageName switch
            {
                LanguageNames.CSharp => (CompilationOptions) new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                LanguageNames.VisualBasic => (CompilationOptions) new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                _ => throw new NotSupportedException($"Language {languageName} is not supported")
            };

        private static ImmutableArray<MetadataReference> CreateMetadataReferences(IReadOnlyCollection<MetadataReference> references)
        {
            var immutableReferencesBuilder = ImmutableArray.CreateBuilder<MetadataReference>();
            if (references != null)
            {
                immutableReferencesBuilder.AddRange(references);
            }

            immutableReferencesBuilder.Add(ReferenceSource.Core);
            immutableReferencesBuilder.Add(ReferenceSource.Linq);
            immutableReferencesBuilder.Add(ReferenceSource.LinqExpression);

            if (ReferenceSource.Core.Display.EndsWith("mscorlib.dll") == false)
            {
                foreach (var netStandardCoreLib in ReferenceSource.NetStandardBasicLibs.Value)
                {
                    immutableReferencesBuilder.Add(netStandardCoreLib);
                }
            }

            return immutableReferencesBuilder.ToImmutable();
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