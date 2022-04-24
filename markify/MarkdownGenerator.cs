using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace markify
{
    /// <summary>
    /// A Generator used to build markdown-formatted documentation pages.
    /// </summary>
    public class MarkdownGenerator : IGenerator
    {

        /// <summary>
        /// Generate a class description in markdown.
        /// </summary>
        /// <param name="name">The name of the class.</param>
        /// <param name="snippet">The code snippet representing the class declaration.</param>
        /// <param name="summary">The summary of the class.</param>
        /// <returns>A string of markdown.</returns>
        public string GenerateClassDescription(string name, string snippet, string summary)
        {
            //build header, namespace info, and snippet
            string output = "# class `" + name + "`\n";
            output += "```csharp\n" + snippet + "\n```\n\n";

            //build summary
            if (!summary.Equals(""))
                output += summary + "\n\n";

            return output;
        }

        public string GenerateInterfaceDescription(string name, string snippet, string summary)
        {
            //build header, namespace info, and snippet
            string output = "# interface `" + name + "`\n";
            output += "```csharp\n" + snippet + "\n```\n\n";

            //build summary
            if (!summary.Equals(""))
                output += summary + "\n\n";

            return output;
        }

        /// <summary>
        /// Generate a struct description in markdown.
        /// </summary>
        /// <param name="name">The name of the struct.</param>
        /// <param name="snippet">The code snippet representing the struct declaration.</param>
        /// <param name="summary">The summary of the class.</param>
        /// <returns>A string of markdown.</returns>
        public string GenerateStructDescription(string name, string snippet, string summary)
        {
            //build header and snippet
            string output = "### struct `" + name + "`\n";
            output += "```csharp\n" + snippet + "\n```\n\n";

            //build summary
            if (!summary.Equals(""))
                output += summary + "\n\n";

            return output;
        }

        /// <summary>
        /// Generate a method description in markdown.
        /// </summary>
        /// <param name="name">The name of the method</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="snippet">The code snippet representing the method declaration.</param>
        /// <param name="summary">The summary of the method. Can be left blank.</param>
        /// <param name="returns">The comment describing what the method returns. Can be left blank.</param>
        /// <param name="parameters">The method's parameters. Can be null.</param>
        /// <param name="typeParameters">The method's type parameters. Can be null.</param>
        /// <param name="paramDict">A Dictionary of parameter names (keys) corresponding with comment descriptions (values).</param>
        /// <param name="typeParamDict">A Dictionary of type parameter names (keys) corresponding with comment descriptions (values).</param>
        /// <param name="headerLevel">A string representing the header level to use when printing the method.</param>
        /// <returns>A string of markdown.</returns>
        public string GenerateMethodDescription(string name, string returnType, string snippet, string summary, string returns, ParameterListSyntax parameters, TypeParameterListSyntax typeParameters, Dictionary<string, string> paramDict, Dictionary<string, string> typeParamDict, string headerLevel = "###")
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

        public string GeneratePropertiesDescription(string property, string comment)
        {
            return "|" + property + "|" + comment + "|\n";
        }

    }
}
