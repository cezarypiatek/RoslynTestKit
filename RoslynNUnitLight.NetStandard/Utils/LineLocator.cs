using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RoslynNUnitLight
{
    class LineLocator : IDiagnosticLocator
    {
        private readonly int line;
        private readonly int _startPosition;
        private readonly int _endPosition;

        public LineLocator(int line, int startPosition, int endPosition)
        {
            this.line = line;
            _startPosition = startPosition;
            _endPosition = endPosition;
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

        public TextSpan GetSpan()
        {
            return new TextSpan(_startPosition, _endPosition);
        }

        public static LineLocator FromCode(string code, int lineNumber)
        {
            var lineStart = code.IndexOf("\n", 0, lineNumber,StringComparison.InvariantCulture);
            var lineEnd = code.IndexOf("\n", 0, lineNumber+1,StringComparison.InvariantCulture);
            return new LineLocator(lineNumber, lineStart, lineEnd);
        }

        public static LineLocator FromDocument(Document document, int lineNumber)
        {
            var sourceCode = document.GetSyntaxRootAsync().GetAwaiter().GetResult().ToFullString();
            return FromCode(sourceCode, lineNumber);
        }
    }
}