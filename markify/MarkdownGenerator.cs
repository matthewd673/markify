using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace markify
{
    public static class MarkdownGenerator
    {

        public static string GenerateClassMarkdown(string name, string snippet, string summary)
        {
            //build header, namespace info, and snippet
            string output = "# class `" + name + "`\n";
            output += "```csharp\n" + snippet + "\n```\n\n";

            //build summary
            if (!summary.Equals(""))
                output += summary + "\n\n";

            return output;
        }

        public static string GenerateStructMarkdown(string name, string snippet, string summary)
        {
            //build header and snippet
            string output = "### struct `" + name + "`\n";
            output += "```csharp\n" + snippet + "\n```\n\n";

            //build summary
            if (!summary.Equals(""))
                output += summary + "\n\n";

            return output;
        }

        public static string GenerateMethodMarkdown(string name, string returnType, string snippet, string summary, string returns, ParameterListSyntax parameters, TypeParameterListSyntax typeParameters, Dictionary<string, string> paramDict, Dictionary<string, string> typeParamDict, string headerLevel = "###")
        {
            //build header and code snippet
            string output = headerLevel + " " + returnType + " `" + name + "`\n";
            output += "```csharp\n" + snippet + "\n```\n\n";

            //build summary
            if (!summary.Equals(""))
                output += summary + "\n\n";

            //build returns info
            if (!returns.Equals(""))
                output += "**Returns:** " + returns + "\n\n";

            //build type parameters table
            if (typeParameters != null && typeParameters.Parameters.Count > 0)
            {
                output += "Type Parameter|Description\n---|---\n";
                foreach (TypeParameterSyntax p in typeParameters.Parameters)
                {
                    output += "**`" + p.Identifier + "`**";
                    //display comment, if it has one
                    if (typeParamDict.ContainsKey(p.Identifier.ToString()))
                        output += " | " + typeParamDict[p.Identifier.ToString()] + "\n";
                    else
                        output += "\n";
                }
                output += "\n";
            }

            //build parameters table
            if (parameters != null && parameters.Parameters.Count > 0)
            {
                output += "Parameter|Description\n---|---\n";
                foreach (ParameterSyntax p in parameters.Parameters)
                {
                    output += "" + p.Type.ToString() + " **`" + p.Identifier + "`**";
                    //display default value, if it has one
                    if (p.Default != null)
                        output += " = `" + p.Default.Value + "`";

                    //display comment, if it has one
                    if (paramDict.ContainsKey(p.Identifier.ToString()))
                        output += " | " + paramDict[p.Identifier.ToString()] + "\n";
                    else
                        output += "\n";
                }
                output += "\n";
            }

            return output;
        }

    }
}
