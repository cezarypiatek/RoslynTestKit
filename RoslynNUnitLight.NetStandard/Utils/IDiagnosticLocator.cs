using Microsoft.CodeAnalysis;

namespace RoslynNUnitLight
{
    public interface IDiagnosticLocator
    {
        bool Match(Location location);
    }
}