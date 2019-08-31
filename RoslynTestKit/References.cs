using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace RoslynTestKit
{
    public static class References
    {
        public static readonly MetadataReference Core = FromType<int>();
        public static readonly MetadataReference Linq = FromType(typeof(Enumerable));
        public static readonly MetadataReference NetStandardCore = 
            MetadataReference.CreateFromFile(((String)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
                .Split(Path.PathSeparator).FirstOrDefault(x=>x.EndsWith("mscorlib.dll")));

        public static MetadataReference FromType<T>()
        {
            return FromType(typeof(T));
        }

        public static MetadataReference FromType(Type type)
        {
            return MetadataReference.CreateFromFile(type.Assembly.Location);
        }
    }
}