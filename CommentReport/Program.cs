using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.IO;

namespace CommentReport
{
    class Program
    {

        static void Main(string[] args)
        {
            //var filename = args[0];
            var filename = @"C:\Source\Repos\PropertyApp\PropertyApp.sln";

            if (!File.Exists(filename))
            {
                Console.WriteLine("Solution file does not exist");
                return;
            }

            var workspace = Microsoft.CodeAnalysis.MSBuild.MSBuildWorkspace.Create();
            var solution = workspace.OpenSolutionAsync(filename).Result;
            
            foreach (var project in solution.Projects)
            {
                var compilation = project.GetCompilationAsync().Result;

                foreach (var tree in compilation.SyntaxTrees)
                {
                    var root = tree.GetRootAsync().Result;
                    DisplayNode(root);
                }
            }

            Console.ResetColor();
        }

        private static void DisplayNode(SyntaxNodeOrToken node, string indent = "")
        {
            if (node.IsToken)
            {
                var token = node.AsToken();

                foreach (var trivia in token.LeadingTrivia)
                {
                    if (trivia.Kind() == SyntaxKind.SingleLineCommentTrivia)
                    {
                        var parent = GetParentName(token.Parent);
                        var comment = token.LeadingTrivia.ToString();

                        Console.ResetColor();
                        Console.WriteLine(parent);

                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(comment.Trim());
                    }
                }
            }

            var children = node.ChildNodesAndTokens();

            foreach (var child in children.OrderBy(t => t.FullSpan))
            {
                DisplayNode(child, indent + "  ");
            }
        }

        private static string GetParentName(SyntaxNode node)
        {
            if (node is PropertyDeclarationSyntax)
            {
                var propNode = node as PropertyDeclarationSyntax;

                var parentName = GetParentName(node.Parent);
                return parentName + "\n" + propNode.Type.GetText().ToString() + propNode.Identifier.Text;
            }
            else if (node is MethodDeclarationSyntax)
            {
                var funcNode = node as MethodDeclarationSyntax;
                var parentName = GetParentName(node.Parent);
                return parentName + "\n" + funcNode.Identifier + "()";
            }
            else if (node is ClassDeclarationSyntax)
            {
                var classNode = node as ClassDeclarationSyntax;
                return "Class " + classNode.Identifier;
            }
            else if (node is UsingDirectiveSyntax)
            {
                var usingNode = node as UsingDirectiveSyntax;
                return "Using " + usingNode.Name;
            }
            else if (node is NamespaceDeclarationSyntax)
            {
                var namespaceNode = node as NamespaceDeclarationSyntax;
                return "Namespace " + namespaceNode.Name;
            }
            else if (node is AttributeListSyntax)
            {
                var parentName = GetParentName(node.Parent);
                var attributeNode = node as AttributeListSyntax;
                return "Attribute " + parentName;
            }
            else
            {
                if (node.Parent != null)
                {
                    return GetParentName(node.Parent);
                }
                else
                {
                    return null;
                }
            }
        }

        private static string GetParentName<T>(SyntaxNode node)
        {
            if (node is T)
            {
                var children = node.ChildTokens().First(t => t.Kind() == SyntaxKind.IdentifierToken);
                return children.Text;
            }
            else
            {
                if (node.Parent == null)
                {
                    return null;
                }
                else
                {
                    return GetParentName<T>(node.Parent);
                }
            }
        }
    }
}
