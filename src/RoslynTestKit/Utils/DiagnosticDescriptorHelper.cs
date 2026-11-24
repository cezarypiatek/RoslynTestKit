using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace RoslynTestKit.Utils
{
    internal static class DiagnosticDescriptorHelper
    {
        public static ReportDiagnostic MapSeverityToReport(this DiagnosticDescriptor descriptor)
        {
            if (descriptor == null)
                throw new Exception("This shouldn't occur");
            return (ReportDiagnostic)descriptor.GetType().GetMethod("MapSeverityToReport", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { descriptor.DefaultSeverity });
        }
    }
}
