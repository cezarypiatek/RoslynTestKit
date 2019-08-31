using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RoslynNUnitLight
{
    public interface IDiagnosticLocator
    {
        bool Match(Location location);
        TextSpan GetSpan();
    }
}