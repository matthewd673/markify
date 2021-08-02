using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
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

            //get namespace
            IEnumerable<NamespaceDeclarationSyntax> namespaceDecsEnum = syntaxTree.GetRoot().DescendantNodes().OfType<NamespaceDeclarationSyntax>();
            NamespaceDeclarationSyntax namespaceDec = namespaceDecsEnum.First();
            string namespaceName = namespaceDec.Name.ToString();

            //get list of all class declarations
            IEnumerable<ClassDeclarationSyntax> classes = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (ClassDeclarationSyntax c in classes)
            {
                //generate markdown for each class
                SyntaxTriviaList triviaList = c.GetLeadingTrivia();

                string rawComment = "";
                foreach (SyntaxTrivia t in triviaList)
                {
                    if (t.Kind() != SyntaxKind.SingleLineDocumentationCommentTrivia)
                        continue;
                    rawComment += t.ToString();
                }

                XmlDocument comment = CommentToXml(rawComment);
                string summary = ParseCommentTag(comment, "summary");

                string classSnippet = BuildClassSnippet(c);

                Console.WriteLine(GenerateClassMarkdown(c.Identifier.Text, namespaceName, classSnippet, summary));

                //generate markdown for everything within the class
                //TODO: move other markdown builders to here
            }

            //get list of all constructors
            IEnumerable<ConstructorDeclarationSyntax> constructors = syntaxTree.GetRoot().DescendantNodes().OfType<ConstructorDeclarationSyntax>();
            foreach (ConstructorDeclarationSyntax c in constructors)
            {
                //TODO
            }

            //get list of all method declarations
            IEnumerable<MethodDeclarationSyntax> methods = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (MethodDeclarationSyntax m in methods)
            {
                SyntaxTriviaList triviaList = m.GetLeadingTrivia();

                string rawComment = "";
                foreach (SyntaxTrivia t in triviaList)
                {
                    if (t.Kind() != SyntaxKind.SingleLineDocumentationCommentTrivia)
                        continue;
                    rawComment += t.ToString();
                }

                XmlDocument comment = CommentToXml(rawComment);
                string summary = ParseCommentTag(comment, "summary");
                string returns = ParseCommentTag(comment, "returns");
                Dictionary<string, string> paramDict = ParseParamComments(comment);

                string methodSnippet = BuildMethodSnippet(m);

                Console.WriteLine(GenerateMethodMarkdown(m.Identifier.Text, m.ReturnType.ToString(), methodSnippet, summary, returns, m.ParameterList, paramDict));
            }

            return output;

        }

        static string GenerateClassMarkdown(string name, string namespaceName, string snippet, string summary)
        {
            //build header, namespace info, and snippet
            string output = "# `" + name + "`\n";
            output += "Namespace: *" + namespaceName + "*\n\n";
            output += "```csharp\n" + snippet + "\n```\n\n";

            //build summary
            if (!summary.Equals(""))
                output += summary + "\n\n";

            return output;
        }

        static string GenerateMethodMarkdown(string name, string returnType, string snippet, string summary, string returns, ParameterListSyntax parameters, Dictionary<string, string> paramDict)
        {
            //build header and code snippet
            string output = "## " + returnType + " `" + name + "`\n";
            output += "```csharp\n" + snippet + "\n```\n\n";

            //build summary
            if (!summary.Equals(""))
                output += summary + "\n\n";

            //build returns info
            if (!returns.Equals(""))
            {
                output += "**Returns:** " + returns + "\n\n";
            }

            //build parameters table
            if (parameters.Parameters.Count > 0)
            {
                output += "Parameter|Description\n---|---\n";
                foreach (ParameterSyntax p in parameters.Parameters)
                {
                    output += "" + p.Type.ToFullString() + " **`" + p.Identifier + "`**";
                    //display default value, if it has one
                    if (p.Default != null)
                        output += " = `" + p.Default.Value + "`";

                    //display comment, if it has one
                    if (paramDict.ContainsKey(p.Identifier.ToString()))
                        output += " | " + paramDict[p.Identifier.ToString()] + "\n";
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

        static XmlDocument CommentToXml(string rawComment)
        {
            string commentXml = "";
            string[] commentLines = rawComment.Split('\n');
            for (int i = 0; i < commentLines.Length; i++)
            {
                string line = commentLines[i].Trim();
                if (line.StartsWith("///"))
                    line = line.Remove(0, 3);
                line = line.Trim();
                commentXml += line + "\n";
            }

            commentXml = "<xml>\n" + commentXml + "\n</xml>"; //one root element

            XmlDocument document = new XmlDocument();
            document.LoadXml(commentXml);

            return document;
        }

        static string ParseCommentTag(XmlDocument document, string tagName)
        {
            XmlNodeList tagList = document.GetElementsByTagName(tagName);

            if (tagList.Count > 0)
                return tagList[0].InnerText;
            else
                return "";
        }

        static Dictionary<string, string> ParseParamComments(XmlDocument document)
        {
            Dictionary<string, string> paramDict = new Dictionary<string, string>();
            foreach (XmlNode node in document.GetElementsByTagName("param"))
            {
                string paramName = node.Attributes.GetNamedItem("name").Value;
                string paramComment = node.InnerText;
                paramDict.Add(paramName, paramComment);
            }

            return paramDict;
        }

    }
}
