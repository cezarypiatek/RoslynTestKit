using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using ApprovalTests.Reporters;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace RoslynTestKit.Utils
{
    public static class DiffHelper
    {
        public static string GenerateInlineDiff(string expected, string actual)
        {
            var differ = new Differ();
            var diffBuilder = new InlineDiffBuilder(differ);
            var diff = diffBuilder.BuildDiffModel(expected, actual, false);
            
            var sb = new StringBuilder();
            var lastChanged = false;
            int? lastLine = null;
            foreach (var line in diff.Lines)
            {
                
                if (line.Type != ChangeType.Unchanged)
                {
                    if (lastChanged == false)
                    {
                        sb.AppendLine("===========================");
                        var linePosition = line.Position ?? lastLine+1;
                        if (linePosition != null) 
                        {
                            sb.AppendLine($"From line {linePosition}:");
                        }
                    }

                    lastChanged = true;
                    sb.Append(GetLinePrefix(line));
                    sb.AppendLine(PresentWhitespaces(line.Text));
                }
                else
                {
                    lastChanged = false;
                }

                lastLine = line.Position;
            }

            return sb.ToString();
        }

        private static string PresentWhitespaces(string lineText)
        {
            return lineText.Replace(' ', '\u00B7')
                .Replace('\t', '\u2192');
        }

        private static string GetLinePrefix(DiffPiece line)
        {
            switch (line.Type)
            {
                case ChangeType.Inserted:
                    return "+ ";
                case ChangeType.Deleted:
                    return "- ";
                case ChangeType.Modified:
                    return "M ";
                case ChangeType.Imaginary:
                    return "I ";
                default:
                    return "  ";
            }
        }

        public static void TryToReportDiff(string expectedCode, string text)
        {
            if (Debugger.IsAttached)
            {
                var tmpDir = Path.GetTempPath();
                var tempFileName = Guid.NewGuid().ToString("N").Substring(0, 4);
                var transformedPath = Path.Combine(tmpDir, $"{tempFileName}_transformed.cs");
                File.WriteAllText(transformedPath, text);
                var expectedPath = Path.Combine(tmpDir, $"{tempFileName}_expected.cs");
                File.WriteAllText(expectedPath, expectedCode);
                new DiffReporter().Report( transformedPath, expectedPath);
            }
        }
    }
}