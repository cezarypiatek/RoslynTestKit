using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RoslynTestKit.Utils
{
    public interface IDiagnosticLocator
    {
        bool Match(Location location);
        TextSpan GetSpan();
    }
}