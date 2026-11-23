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
            return location.IsInSource && _textSpan.Start == location.SourceSpan.Start && _textSpan.End == location.SourceSpan.End;
        }

        public TextSpan GetSpan()
        {
            return _textSpan;
        }

        public string Description() => $"[{_textSpan.Start}...{_textSpan.End}]";
    }
}