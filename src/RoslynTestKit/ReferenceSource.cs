using System;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyModel;

namespace RoslynTestKit
{
    public static class ReferenceSource
    {
        internal static readonly MetadataReference[] References;

        static ReferenceSource()
        {
            References = DependencyContext.Default.CompileLibraries.SelectMany(cl => cl.ResolveReferencePaths())
                                                                   .Select(rp => MetadataReference.CreateFromFile(rp) as MetadataReference)
                                                                   .ToArray();
        }

        public static MetadataReference FromType<T>() => FromType(typeof(T));

        public static MetadataReference FromAssembly(Assembly assembly) => FromAssembly(assembly.Location);

        public static MetadataReference FromAssembly(string assemblyLocation) => MetadataReference.CreateFromFile(assemblyLocation);

        public static MetadataReference FromType(Type type) => MetadataReference.CreateFromFile(type.Assembly.Location);
    }
}