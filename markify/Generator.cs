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
                output += n.Identifier + "\n";

                string[] modifiers = new string[n.Modifiers.Count];
                for (int i = 0; i < modifiers.Length; i++)
                {
                    modifiers[i] = n.Modifiers[0].ToString();
                }

                SyntaxTriviaList triviaList = n.GetLeadingTrivia();
                string commentXml = triviaList[0].ToString();

                MethodInfo methodInfo = new MethodInfo(n.Identifier.Text, n.ReturnType.ToString(), modifiers, n.ParameterList, n.TypeParameterList, commentXml);
            }

            return output;

        }

        struct MethodInfo
        {
            public string name;
            public string returnType;
            public string[] modifiers;
            public ParameterListSyntax parameters;
            public TypeParameterListSyntax typeParameters;
            public string commentXml;

            public MethodInfo(string name, string returnType, string[] modifiers, ParameterListSyntax parameters, TypeParameterListSyntax typeParameters, string commentXml)
            {
                this.name = name;
                this.returnType = returnType;
                this.modifiers = modifiers;
                this.parameters = parameters;
                this.typeParameters = typeParameters;
                this.commentXml = commentXml;
            }

        }

        struct ParamInfo
        {
            public string name;
            public string type;
            public string comment;

            public ParamInfo(string name, string type, string comment)
            {
                this.name = name;
                this.type = type;
                this.comment = comment;
            }
        }

    }
}
