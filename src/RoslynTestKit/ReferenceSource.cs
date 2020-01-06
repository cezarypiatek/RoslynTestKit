using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace RoslynTestKit
{
    public static class ReferenceSource
    {
        internal static readonly MetadataReference Core = FromType<int>();
        internal static readonly MetadataReference Linq = FromType(typeof(Enumerable));
        public static readonly MetadataReference NetStandardCore;

        static ReferenceSource()
        {
            var trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
            if (trustedPlatformAssemblies != null)
            {
                NetStandardCore = MetadataReference.CreateFromFile(((String)trustedPlatformAssemblies)
                    ?.Split(Path.PathSeparator).FirstOrDefault(x => x.EndsWith("mscorlib.dll")));
            }
            else
            {
                NetStandardCore = null;
            }

        }

        public static MetadataReference FromType<T>() => FromType(typeof(T));

        public static MetadataReference FromAssembly(Assembly assembly) => FromAssembly(assembly.Location);

        public static MetadataReference FromAssembly(string assemblyLocation) => MetadataReference.CreateFromFile(assemblyLocation);

        public static MetadataReference FromType(Type type) => MetadataReference.CreateFromFile(type.Assembly.Location);
    }
}