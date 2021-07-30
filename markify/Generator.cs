using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace markify
{
    public static class Generator
    {

        public static string ParseFile(string sourceText)
        {

            string output = "";

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceText, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Parse));
            IEnumerable<MethodDeclarationSyntax> nodes = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (MethodDeclarationSyntax n in nodes)
            {
                SyntaxTriviaList triviaList = n.GetLeadingTrivia();
                output += n.Identifier + "\n";
                foreach (SyntaxTrivia t in triviaList)
                {
                    if (t.Kind() != SyntaxKind.SingleLineDocumentationCommentTrivia)
                        continue;
                    else
                        output += t.GetStructure() + "\n";
                }
                output += "---------------------\n";
                //output += n.Identifier + " " + n.GetLeadingTrivia() + "\n";
                //output += n.Identifier + " (" + n.ReturnType + ") " + n.Modifiers + ", " + n.ParameterList + "\n";
            }

            return output;

        }

        struct MethodInfo
        {
            public string name;
            public string returnType;
            public string modifiers;
            public List<ParamInfo> paramInfoList;
            public string comment;
        }

        struct ParamInfo
        {
            public string name;
            public string type;
            public string comment;
        }

    }
}
