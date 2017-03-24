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
            var filename = @"C:\Source\AnalysisTest\AnalysisTest.sln";

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
                    var nodes = root.DescendantNodesAndSelf();
                    var comments = nodes.Where(node => node.IsKind(SyntaxKind.SingleLineCommentTrivia) || node.IsKind(SyntaxKind.MultiLineCommentTrivia));

                    foreach (var comment in comments)
                    {
                        Console.WriteLine(comment.FullSpan.ToString());
                    }
                }
            }
        }
    }
}
