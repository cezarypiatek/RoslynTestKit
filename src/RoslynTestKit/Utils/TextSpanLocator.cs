using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RoslynTestKit.Utils
{
    class TextSpanLocator : IDiagnosticLocator
    {
        private TextSpan _textSpan;

        public TextSpanLocator(TextSpan textSpan)
        {
            _textSpan = textSpan;
        }

        public bool Match(Location location)
        {
            return location.IsInSource &&  _textSpan.IntersectsWith(location.SourceSpan);
        }

        public TextSpan GetSpan()
        {
            return _textSpan;
        }

        public string Description() => $"[{_textSpan.Start}...{_textSpan.End}]";
    }
}
