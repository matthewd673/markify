using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace markify
{
    /// <summary>
    /// An interface describing a generator, which can build custom output strings for information parsed from a C# file.
    /// </summary>
    public interface IGenerator
    {
        string GenerateClassDescription(string name, string snippet, string summary);
        string GenerateInterfaceDescription(string name, string snippet, string summary);
        string GenerateStructDescription(string name, string snippet, string summary);
        string GenerateMethodDescription(string name, string returnType, string snippet,
            string summary, string returns, ParameterListSyntax parameters,
            TypeParameterListSyntax typeParameters, Dictionary<string, string> paramDict,
            Dictionary<string, string> typeParamDict, string headerLevel = "###");
    }
}
