using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;

namespace RoslynTestKit
{
    public abstract class BaseTestFixture
    {
        protected abstract string LanguageName { get; }

        protected virtual bool ThrowsWhenInputDocumentContainsError { get; } = false;

        protected virtual IReadOnlyCollection<MetadataReference> References => null;


        protected Document CreateDocumentFromCode(string code)
        {
            return CreateDocumentFromCode(code, LanguageName, References ?? Array.Empty<MetadataReference>());
        }

        /// <summary>
        ///     Should create the compilation and return a document that represents the provided code
        /// </summary>
        protected virtual Document CreateDocumentFromCode(string code, string languageName, IReadOnlyCollection<MetadataReference> extraReferences)
        {
            var frameworkReferences = CreateFrameworkMetadataReferences();

            var compilationOptions = GetCompilationOptions(languageName);

            return new AdhocWorkspace()
                .AddProject("TestProject", languageName)
                .WithCompilationOptions(compilationOptions)
                .AddMetadataReferences(frameworkReferences)
                .AddMetadataReferences(extraReferences)
                .AddDocument("TestDocument", code);
        }

        private static CompilationOptions GetCompilationOptions(string languageName) =>
            languageName switch
            {
                LanguageNames.CSharp => new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                LanguageNames.VisualBasic => new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                _ => throw new NotSupportedException($"Language {languageName} is not supported")
            };

        protected virtual IEnumerable<MetadataReference> CreateFrameworkMetadataReferences()
        {
            yield return ReferenceSource.Core;
            yield return ReferenceSource.Linq;
            yield return ReferenceSource.LinqExpression;

            if (ReferenceSource.Core.Display.EndsWith("mscorlib.dll") == false)
            {
                foreach (var netStandardCoreLib in ReferenceSource.NetStandardBasicLibs.Value)
                {
                    yield return netStandardCoreLib;
                }
            }
        }
    }
}
