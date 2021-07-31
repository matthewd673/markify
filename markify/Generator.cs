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

            //get list of all class declarations
            IEnumerable<ClassDeclarationSyntax> classes = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (ClassDeclarationSyntax c in classes)
            {
                string classSnippet = BuildClassSnippet(c);

                ClassInfo classInfo = new ClassInfo(c.Identifier.Text, classSnippet);
                Console.WriteLine(GenerateClassMarkdown(classInfo));
            }

            //get list of all method declarations
            IEnumerable<MethodDeclarationSyntax> methods = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (MethodDeclarationSyntax m in methods)
            {
                SyntaxTriviaList triviaList = m.GetLeadingTrivia();
                string commentXml = triviaList[0].ToString();

                string methodSnippet = BuildMethodSnippet(m);

                MethodInfo methodInfo = new MethodInfo(m.Identifier.Text, m.ReturnType.ToString(), m.ParameterList, m.TypeParameterList, commentXml, methodSnippet);
                Console.WriteLine(GenerateMethodMarkdown(methodInfo));
            }

            return output;

        }

        static string GenerateClassMarkdown(ClassInfo info)
        {
            string output = "# `" + info.name + "`\n";
            output += "`" + info.snippet + "`\n";

            return output;
        }

        static string GenerateMethodMarkdown(MethodInfo info)
        {
            string output = "## " + info.returnType + " `" + info.name + "`\n";
            output += "`" + info.snippet + "`\n";

            if (info.parameters.Parameters.Count > 0)
            {
                output += "### Parameters\n";
                foreach (ParameterSyntax p in info.parameters.Parameters)
                {
                    output += "- " + p.Type.ToString() + " **`" + p.Identifier + "`**";
                    if (p.Default != null)
                        output += ": " + p.Default.Value + "\n";
                    else
                        output += "\n";
                }
            }
            else
            {
                output += "*(No parameters)*\n";
            }

            return output;
        }

        static string BuildClassSnippet(ClassDeclarationSyntax c)
        {
            string output = "";
            
            //start with the first token and continue until an open brace is found, marking beginning of block
            //this is implemented very differently from BuildMethodSnippet because ChildNodes() seems to only return the block for a ClassDeclarationSyntax
            SyntaxToken current = c.GetFirstToken();
            while (true)
            {
                if (current.Kind() == SyntaxKind.OpenBraceToken)
                    break;
                output += current.ToString() + " ";
                current = current.GetNextToken();
            }
            return output.Trim();
        }

        static string BuildMethodSnippet(MethodDeclarationSyntax m)
        {
            List<SyntaxNode> children = m.ChildNodes().ToList();

            //build output starting with modifiers, then next child nodes until block
            string output = m.Modifiers.ToString().Trim() + " ";
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].IsKind(SyntaxKind.Block)) //stop at code block
                    break;

                if (i == 1) //add identifier when appropriate
                    output += m.Identifier;

                if (children[i].IsKind(SyntaxKind.ParameterList)) //avoid whitespace before parameter list (cosmetic)
                    output = output.Trim();

                output += children[i] + " ";
            }

            return output.Trim();
        }

        struct ClassInfo
        {
            public string name;
            public string snippet;

            public ClassInfo(string name, string snippet)
            {
                this.name = name;
                this.snippet = snippet;
            }            
        }

        struct MethodInfo
        {
            public string name;
            public string returnType;
            public ParameterListSyntax parameters;
            public TypeParameterListSyntax typeParameters;
            public string commentXml;
            public string snippet;

            public MethodInfo(string name, string returnType, ParameterListSyntax parameters, TypeParameterListSyntax typeParameters, string commentXml, string snippet)
            {
                this.name = name;
                this.returnType = returnType;
                this.parameters = parameters;
                this.typeParameters = typeParameters;
                this.commentXml = commentXml;
                this.snippet = snippet;
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
