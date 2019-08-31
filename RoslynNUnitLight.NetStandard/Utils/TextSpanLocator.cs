using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RoslynNUnitLight
{
    class TextSpanLocator : IDiagnosticLocator
    {
        private readonly TextSpan _textSpan;

        public TextSpanLocator(TextSpan textSpan)
        {
            _textSpan = textSpan;
        }

        public bool Match(Location location)
        {
            return location.IsInSource && location.SourceSpan == _textSpan;
        }

        public TextSpan GetSpan()
        {
            return _textSpan;
        }
    }
}