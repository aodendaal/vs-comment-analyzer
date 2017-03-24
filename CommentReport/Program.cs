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

            var projects = solution.Projects;

            foreach (var project in projects)
            {
                var compilation = project.GetCompilationAsync().Result;

                foreach (var tree in compilation.SyntaxTrees)
                {
                    var root = tree.GetRootAsync().Result;
                    DisplayNode(root);
                }

                //var tree = compilation.SyntaxTrees.First().GetRootAsync().Result;

                //var root = tree.DescendantNodesAndSelf().OfType<NamespaceDeclarationSyntax>().First();

                //DisplayNode(root);
            }

            Console.ResetColor();
        }

        private static void DisplayNode(SyntaxNodeOrToken node, string indent = "")
        {
            if (node.IsNode)
            {
                var childNode = node.AsNode();
                var name = childNode.GetType().ToString();
                name = name.Replace("Microsoft.CodeAnalysis.CSharp.Syntax.", "");
                name = name.Replace("Syntax", "");
                //Console.ForegroundColor = ConsoleColor.Blue;
                //Console.Write($"{indent} {name} ");

                Console.ResetColor();
                if (childNode is NamespaceDeclarationSyntax)
                {
                    //Console.WriteLine((childNode as NamespaceDeclarationSyntax).Name);
                }
                else if (childNode is UsingDirectiveSyntax)
                {
                    //Console.WriteLine((childNode as UsingDirectiveSyntax).Name);
                    return;
                }
                else if (childNode is ClassDeclarationSyntax)
                {
                    //Console.WriteLine((childNode as ClassDeclarationSyntax).Identifier);
                    //return;
                }
                else if (childNode is MethodDeclarationSyntax)
                {
                    //Console.WriteLine((childNode as MethodDeclarationSyntax).Identifier);
                }
                else
                {
                    //Console.WriteLine();
                }
            }
            else
            {
                var token = node.AsToken();

                foreach (var trivia in token.LeadingTrivia)
                {
                    if (trivia.Kind() == SyntaxKind.SingleLineCommentTrivia)
                    {
                        //var parentNode = token.Parent;
                        //Console.ForegroundColor = ConsoleColor.Blue;
                        //Console.WriteLine($"{indent} {parentNode.GetType()}");

                        //Console.ForegroundColor = ConsoleColor.Green;
                        //Console.Write($"{indent} {token.Kind()} ");
                        //Console.ResetColor();
                        //Console.WriteLine(token.Text);

                        //var propertyId = GetParentName<PropertyDeclarationSyntax>(token.Parent);
                        //var methodId = GetParentName<MethodDeclarationSyntax>(token.Parent);
                        //var classId = GetParentName<ClassDeclarationSyntax>(token.Parent);
                        var parent = GetParentName(token.Parent);
                        var comment = token.LeadingTrivia.ToString();

                        Console.ResetColor();
                        Console.WriteLine(parent);
                        //Console.ForegroundColor = ConsoleColor.Blue;
                        //Console.WriteLine(classId);
                        //Console.ForegroundColor = ConsoleColor.Green;
                        //Console.WriteLine((methodId != null) ? methodId : propertyId);
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(comment.Trim());

                        //Console.ForegroundColor = ConsoleColor.DarkRed;
                        //Console.Write($"'{comment.Trim()}'\t");
                        //Console.ResetColor();
                        //Console.Write($"{methodId}\t{classId}");

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
                return propNode.Type.GetText().ToString() + " " + propNode.Identifier.Text;
            }
            else if (node is MethodDeclarationSyntax)
            {
                var funcNode = node as MethodDeclarationSyntax;
                return funcNode.Identifier + "()";
            }
            else if (node is ClassDeclarationSyntax)
            {
                var classNode = node as ClassDeclarationSyntax;
                return "class " + classNode.Identifier;
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
