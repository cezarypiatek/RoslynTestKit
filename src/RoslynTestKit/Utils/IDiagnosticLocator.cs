using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace RoslynTestKit.Utils
{
    public interface IDiagnosticLocator
    {
        bool Match(Location location);
        TextSpan GetSpan();

        string Description();
    }
}