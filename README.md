# RoslynNUnitLight

A lightweight framework for writing unit tests for Roslyn diagnostic
analyzers, code fixes and refactorings using NUnit.

### Quick Start

1. Install the [RoslynNUnitLight](https://www.nuget.org/packages/RoslynNUnitLight)
   package from NuGet into your project.
2. Create a new class that inherits from one of the provided ```*TestFixture```
   classes that matches what are going to test.

   * [```DiagnosticAnalyzer```](http://source.roslyn.io/#Microsoft.CodeAnalysis/DiagnosticAnalyzer/DiagnosticAnalyzer.cs) = ```AnalyzerTestFixture``` 
   * [```CodeFixProvider```](http://source.roslyn.io/#Microsoft.CodeAnalysis.Workspaces/CodeFixes/CodeFixProvider.cs) = ```CodeFixTestFixture```
   * [```CodeRefactoringProvider```](http://source.roslyn.io/#Microsoft.CodeAnalysis.Workspaces/CodeRefactorings/CodeRefactoringProvider.cs) = ```CodeRefactoringTestFixture``` 

3. Override the ```LanguageName``` property and return the appropriate value
   from [```Microsoft.CodeAnalysis.LanguageNames```](http://source.roslyn.io/#Microsoft.CodeAnalysis/Symbols/LanguageNames.cs),
   depending on what language your tests will target.
4. Override the ```CreateAnalyzer``` or ```CreateProvider``` method and return
   an instance of your analyzer or provider.
5. Write tests!

### Writing Unit Tests

RoslynNUnitLight accepts strings that are marked up with ```[|``` and ```|]```
to identify a particular span. This could represent the span of an expected
diagnostic or the text selection before a refactoring is applied.

#### Example: Test presence of a diagnostic

```C#
[Test]
public void AutoPropDeclaredAndUsedInConstructor()
{
    const string code = @"
class C
{
	public bool MyProperty { get; [|private set;|] }
	public C(bool f)
	{
		MyProperty = f;
	}
}";

    HasDiagnostic(code, DiagnosticIds.UseGetterOnlyAutoProperty);
}
```

#### Example: Test absence of a diagnostic

```C#
[Test]
public void AutoPropAlreadyReadonly()
{
    const string code = @"
class C
{
    public bool MyProperty { get; }
    public C(bool f)
    {
        MyProperty = f;
    }
}";

    NoDiagnostic(code, DiagnosticIds.UseGetterOnlyAutoProperty);
}
```

#### Example: Test code fix behavior

```C#
[Test]
public void TestSimpleProperty()
{
    const string markupCode = @"
class C
{
    public bool P1 { get; [|private set;|] }
}";

    const string expected = @"
class C
{
    public bool P1 { get; }
}";

    TestCodeFix(markupCode, expected, DiagnosticDescriptors.UseGetterOnlyAutoProperty);
}
```

#### Example: Test code refactoring behavior

```C#
[Test]
public void SimpleTest()
{
    const string markupCode = @"
class C
{
    void M()
    {
        var s = [|string.Format(""{0}"", 42)|];
    }
}";

    const string expected = @"
class C
{
    void M()
    {
        var s = $""{42}"";
    }
}";

    TestCodeRefactoring(markupCode, expected);
}
```