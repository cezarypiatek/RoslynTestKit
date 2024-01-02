using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RoslynTestKit
{
    public static class RoslynFixtureFactory
    {
        public static AnalyzerTestFixture Create<T>(AnalyzerTestFixtureConfig? config = null) 
            where T : DiagnosticAnalyzer, new()
        {
            var diagnosticAnalyzer = new T();
            return Create(diagnosticAnalyzer, config);
        }

        public static AnalyzerTestFixture Create<T>(T diagnosticAnalyzer, AnalyzerTestFixtureConfig? config = null) 
            where T : DiagnosticAnalyzer
        {
         
            return new ConfigurableAnalyzerTestFixture(diagnosticAnalyzer, config ?? new AnalyzerTestFixtureConfig());
        }
        
        public static CodeFixTestFixture Create<T>(CodeFixTestFixtureConfig? config = null) 
            where T : CodeFixProvider, new()
        {
            var codeFixProvider = new T();
            return Create(codeFixProvider, config);
        }

        private static CodeFixTestFixture Create<T>(T codeFixProvider, CodeFixTestFixtureConfig? config = null) 
            where T : CodeFixProvider
        {
            return new ConfigurableCodeFixTestFixture(codeFixProvider, config ?? new CodeFixTestFixtureConfig());
        }
        
        public static CodeRefactoringTestFixture Create<T>(CodeRefactoringTestFixtureConfig? config = null) 
            where T : CodeRefactoringProvider, new()
        {
            var codeRefactoringProvider = new T();
            return Create(codeRefactoringProvider, config);
        }

        private static CodeRefactoringTestFixture Create<T>(T codeRefactoringProvider, CodeRefactoringTestFixtureConfig? config = null) 
            where T : CodeRefactoringProvider
        {
            return new ConfigurableCodeRefactoringTestFixture(codeRefactoringProvider, config ?? new CodeRefactoringTestFixtureConfig());
        }
        
        
        public static CompletionProviderFixture Create<T>(CompletionProviderTestFixtureConfig? config = null) 
            where T : CompletionProvider, new()
        {
            var codeRefactoringProvider = new T();
            return Create(codeRefactoringProvider, config);
        }

        private static ConfigurableCompletionProviderTestFixture Create<T>(T completionProvider, CompletionProviderTestFixtureConfig? config = null) 
            where T : CompletionProvider
        {
            return new ConfigurableCompletionProviderTestFixture(completionProvider, config ??  new CompletionProviderTestFixtureConfig());
        }
    }
}