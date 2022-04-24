using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace markify
{
    public class Parser
    {

        IGenerator generator;

        /// <summary>
        /// Initialize a new Parser.
        /// </summary>
        /// <param name="generator">The generator to use when building the output.</param>
        public Parser(IGenerator generator)
        {
            this.generator = generator;
        }

        /// <summary>
        /// Parse a given C# file, and return the full generated output.
        /// </summary>
        /// <param name="sourceText">The contents of the C# file to build from.</param>
        /// <returns>A string representing the full output of the generator.</returns>
        public string ParseFile(string sourceText)
        {
            string output = "";

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceText, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Parse));

            //get namespace
            IEnumerable<NamespaceDeclarationSyntax> namespaceDecsEnum = syntaxTree.GetRoot().DescendantNodes().OfType<NamespaceDeclarationSyntax>();
            NamespaceDeclarationSyntax namespaceDec = namespaceDecsEnum.First();

            output += "**Namespace:** " + namespaceDec.Name.ToString() + "\n\n";

            //parse all class declarations
            IEnumerable<ClassDeclarationSyntax> classes = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
            int classI = 0;
            foreach (ClassDeclarationSyntax c in classes)
            {
                //CLASS DECLARATION
                output += ParseClassDeclarationSyntax(c);

                IEnumerable<StructDeclarationSyntax> structs = c.DescendantNodes().OfType<StructDeclarationSyntax>();
                IEnumerable<ConstructorDeclarationSyntax> constructors = c.DescendantNodes().OfType<ConstructorDeclarationSyntax>();
                IEnumerable<MethodDeclarationSyntax> methods = c.DescendantNodes().OfType<MethodDeclarationSyntax>();
                IEnumerable<PropertyDeclarationSyntax> properties = c.DescendantNodes().OfType<PropertyDeclarationSyntax>();

                //build navigation
                output += "**Navigate**\n";
                if (properties.Any())
                    output += "- [Properties](#properties)\n";
                if (structs.Any())
                    output += "- [Structs](#structs)\n";
                if (constructors.Any())
                    output += "- [Constructors](#constructors)\n";
                if (methods.Any())
                    output += "- [Methods](#methods)\n\n";

                //PROPERTIES
                //add section header
                if (properties.Any())
                {
                    output += "## Properties\n";
                    output += "|Property|Description|\n|---|---|\n";
                }
                foreach (PropertyDeclarationSyntax p in properties)
                {
                    output += ParsePropertiesDeclarationSyntax(p);
                }

                //STRUCTS
                //add section header
                if (structs.Any())
                    output += "## Structs\n";
                //parse all structs
                foreach (StructDeclarationSyntax s in structs)
                {
                    output += ParseStructDeclarationSyntax(s);

                    IEnumerable<ConstructorDeclarationSyntax> structConstructors = s.DescendantNodes().OfType<ConstructorDeclarationSyntax>();
                    foreach (ConstructorDeclarationSyntax con in structConstructors)
                        output += ParseConstructorDeclarationSyntax(con, structConstructor: true);
                }

                //CONSTRUCTORS
                //add section header
                if (constructors.Any())
                    output += "## Constructors\n";
                //parse all constructors
                foreach (ConstructorDeclarationSyntax con in constructors)
                {
                    if (con.Parent.Kind() == SyntaxKind.StructDeclaration)
                        continue;
                    output += ParseConstructorDeclarationSyntax(con);
                }

                //METHODS
                //add section header
                if (methods.Any())
                    output += ("## Methods\n");
                //parse all method declarations
                foreach (MethodDeclarationSyntax m in methods)
                {
                    output += ParseMethodDeclarationSyntax(m);
                }

                //add break, if necessary
                classI++;
                if (classI < classes.Count())
                    output += "---\n\n";

            }

            //parse all interface declarations
            IEnumerable<InterfaceDeclarationSyntax> interfaces = syntaxTree.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>();
            int interI = 0;
            foreach (InterfaceDeclarationSyntax i in interfaces) //copied from previous
            {
                //CLASS DECLARATION
                output += ParseInterfaceDeclarationSyntax(i);

                IEnumerable<StructDeclarationSyntax> structs = i.DescendantNodes().OfType<StructDeclarationSyntax>();
                IEnumerable<ConstructorDeclarationSyntax> constructors = i.DescendantNodes().OfType<ConstructorDeclarationSyntax>();
                IEnumerable<MethodDeclarationSyntax> methods = i.DescendantNodes().OfType<MethodDeclarationSyntax>();

                //build navigation
                output += "**Navigate**\n";
                if (structs.Any())
                    output += "- [Structs](#structs)\n";
                if (constructors.Any())
                    output += "- [Constructors](#constructors)\n";
                if (methods.Any())
                    output += "- [Methods](#methods)\n\n";

                //STRUCTS
                //add section header
                if (structs.Any())
                    output += "## Structs\n";
                //parse all structs
                foreach (StructDeclarationSyntax s in structs)
                {
                    output += ParseStructDeclarationSyntax(s);

                    IEnumerable<ConstructorDeclarationSyntax> structConstructors = s.DescendantNodes().OfType<ConstructorDeclarationSyntax>();
                    foreach (ConstructorDeclarationSyntax con in structConstructors)
                        output += ParseConstructorDeclarationSyntax(con, structConstructor: true);
                }

                //CONSTRUCTORS
                //add section header
                if (constructors.Any())
                    output += "## Constructors\n";
                //parse all constructors
                foreach (ConstructorDeclarationSyntax con in constructors)
                {
                    if (con.Parent.Kind() == SyntaxKind.StructDeclaration)
                        continue;
                    output += ParseConstructorDeclarationSyntax(con);
                }

                //METHODS
                //add section header
                if (methods.Any())
                    output += ("## Methods\n");
                //parse all method declarations
                foreach (MethodDeclarationSyntax m in methods)
                    output += ParseMethodDeclarationSyntax(m);

                //add break, if necessary
                interI++;
                if (interI < interfaces.Count())
                    output += "---\n\n";
            }

            return output;

        }

        static string ParseLeadingSummaryComment(CSharpSyntaxNode node)
        {
            SyntaxTriviaList triviaList = node.GetLeadingTrivia();

            string rawComment = "";
            foreach (SyntaxTrivia t in triviaList)
            {
                if (t.Kind() != SyntaxKind.SingleLineDocumentationCommentTrivia)
                    continue;
                rawComment += t.ToString();
            }

            XmlDocument comment = CommentToXml(rawComment);
            string summary = ParseCommentTag(comment, "summary");

            return summary;
        }

        string ParsePropertiesDeclarationSyntax(PropertyDeclarationSyntax p)
        {
            //string summary = ParseLeadingSummaryComment(p);

            string summary = "";
            foreach (SyntaxTrivia t in p.GetLeadingTrivia())
            {
                if (t.Kind() != (SyntaxKind.SingleLineCommentTrivia))
                    continue;
                string description = t.ToString();
                if (description.StartsWith("//"))
                    description = description.Remove(0, 2);
                summary += description.Trim();
            }

            return generator.GeneratePropertiesDescription(p.Identifier.Text, summary);
        }

        string ParseClassDeclarationSyntax(ClassDeclarationSyntax c)
        {
            //generate markdown for each class
            string summary = ParseLeadingSummaryComment(c);
            string classSnippet = BuildClassSnippet(c);

            return generator.GenerateClassDescription(c.Identifier.Text, classSnippet, summary);
        }

        string ParseInterfaceDeclarationSyntax(InterfaceDeclarationSyntax i)
        {
            //generate markdown for each interface
            string summary = ParseLeadingSummaryComment(i);
            string classSnippet = BuildInterfaceSnippet(i);

            return generator.GenerateInterfaceDescription(i.Identifier.Text, classSnippet, summary);
        }

        string ParseStructDeclarationSyntax(StructDeclarationSyntax s)
        {
            string summary = ParseLeadingSummaryComment(s);
            string structSnippet = BuildStructSnippet(s);

            return generator.GenerateStructDescription(s.Identifier.Text, structSnippet, summary);
        }

        string ParseConstructorDeclarationSyntax(ConstructorDeclarationSyntax c, bool structConstructor = false)
        {
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
            Dictionary<string, string> paramDict = ParseParamComments(comment);

            string constructorSnippet = BuildConstructorSnippet(c);

            string headerLevel = "###";
            if (structConstructor)
                headerLevel = "####";

            return generator.GenerateMethodDescription(c.Identifier.Text, "public", constructorSnippet, summary, "", c.ParameterList, null, paramDict, null, headerLevel);
        }

        string ParseMethodDeclarationSyntax(MethodDeclarationSyntax m)
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
            Dictionary<string, string> typeParamDict = ParseParamComments(comment, "typeparam");

            string methodSnippet = BuildMethodSnippet(m);

            return generator.GenerateMethodDescription(m.Identifier.Text, m.ReturnType.ToString(), methodSnippet, summary, returns, m.ParameterList, m.TypeParameterList, paramDict, typeParamDict);
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

        //Copied from BuildClassSnippet
        static string BuildInterfaceSnippet(InterfaceDeclarationSyntax i)
        {
            string output = "";

            //start with the first token and continue until an open brace is found, marking beginning of block
            //this is implemented very differently from BuildMethodSnippet because ChildNodes() seems to only return the block for a ClassDeclarationSyntax
            SyntaxToken current = i.GetFirstToken();
            while (true)
            {
                if (current.Kind() == SyntaxKind.OpenBraceToken)
                    break;
                output += current.ToString() + " ";
                current = current.GetNextToken();
            }
            return output.Trim();
        }

        static string BuildStructSnippet(StructDeclarationSyntax s)
        {
            string output = "";

            //copied from BuildClassSnippet
            SyntaxToken current = s.GetFirstToken();
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

        //copied from BuildMethodSnippet
        static string BuildConstructorSnippet(ConstructorDeclarationSyntax c)
        {
            List<SyntaxNode> children = c.ChildNodes().ToList();

            //build output starting with modifiers, then next child nodes until block
            string output = c.Modifiers.ToString().Trim() + " ";
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].IsKind(SyntaxKind.Block)) //stop at code block
                    break;

                if (i == 0) //add identifier when appropriate
                    output += c.Identifier;

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

        static Dictionary<string, string> ParseParamComments(XmlDocument document, string tagName = "param")
        {
            Dictionary<string, string> paramDict = new Dictionary<string, string>();
            foreach (XmlNode node in document.GetElementsByTagName(tagName))
            {
                string paramName = node.Attributes.GetNamedItem("name").Value;
                string paramComment = node.InnerText;
                paramDict.Add(paramName, paramComment);
            }

            return paramDict;
        }



    }
}
