using Microsoft.CodeAnalysis;

namespace RoslynNUnitLight
{
    class LineLocator : IDiagnosticLocator
    {
        private readonly int line;

        public LineLocator(int line)
        {
            this.line = line;
        }

        public bool Match(Location location)
        {
            var zeroBaseLine = line - 1;
            var lineSpan = location.GetLineSpan();
            return
                location.IsInSource &&
                lineSpan.StartLinePosition.Line <= zeroBaseLine &&
                lineSpan.EndLinePosition.Line >= zeroBaseLine;
        }
    }
}