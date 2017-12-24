using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cs2hx;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Test
{
    public static class TestFramework
    {

        public static void TestCode(string testName, string cSharp, string expectedOutput)
        {
            TestCode(testName, new[] { cSharp }, new[] { expectedOutput });
        }
        public static void TestCode(string testName, string cSharp, IEnumerable<string> expectedOutput)
        {
            TestCode(testName, new[] { cSharp }, expectedOutput);
        }
        public static void TestCode(string testName, IEnumerable<string> cSharp, string expectedOutput)
        {
            TestCode(testName, cSharp, new[] { expectedOutput });
        }
        public static void TestCode(string testName, IEnumerable<string> cSharp, IEnumerable<string> expectedOutput, params string[] extraTranslation)
        {
			var dir = Path.Combine(Path.GetTempPath(), "CS2HX", testName);

			if (Directory.Exists(dir))
				foreach (var existing in Directory.GetFiles(dir, "*.hx", SearchOption.AllDirectories))
				{
					File.SetAttributes(existing, FileAttributes.Normal); //clear read only flag so we can delete it
					File.Delete(existing);
				}

            Console.WriteLine("Parsing into " + dir);

            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location); //https://roslyn.codeplex.com/discussions/541557

            var compilation = CSharpCompilation.Create(testName, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)) //dll so we don't require a main method
				.AddReferences(
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll"))
                )
                .AddSyntaxTrees(cSharp.Select(o => CSharpSyntaxTree.ParseText(o)));

            Cs2hx.Program.Go(compilation, dir, extraTranslation, null);

            Func<string, string> strip = i => Regex.Replace(i, "[\r\n \t]+", " ").Trim();

            var haxeFilesFromDisk = Directory.GetFiles(dir, "*.hx", SearchOption.AllDirectories)
                .Where(o => Path.GetFileName(o) != "Main.hx" && Path.GetFileName(o) != "Constructors.hx")
                .Select(File.ReadAllText)
                .Select(strip)
                .OrderBy(o => o)
                .ToList();

            var expectedOutputStripped = expectedOutput.Select(o => strip(o))
                .OrderBy(o => o)
                .ToList();

            Assert.AreEqual(haxeFilesFromDisk.Count, expectedOutputStripped.Count);

            for (int i = 0; i < expectedOutputStripped.Count; i++)
            {
                if (expectedOutputStripped[i] != haxeFilesFromDisk[i])
                {
					var err = new StringBuilder();

					err.AppendLine("Code different");
                    err.AppendLine("---------------Expected----------------");
                    err.AppendLine(expectedOutputStripped[i]);
                    err.AppendLine("---------------Actual----------------");
                    err.AppendLine(haxeFilesFromDisk[i]);

					var at = DifferentAt(expectedOutputStripped[i], haxeFilesFromDisk[i]);
					err.AppendLine("Different at " + at);

					var sub = at - 15;
					if (sub > 0)
					{

						err.AppendLine("---------------Expected after " + sub + "----------------");
						err.AppendLine(expectedOutputStripped[i].SubstringSafe(sub, 30));
						err.AppendLine("---------------Actual after " + sub + "----------------");
						err.AppendLine(haxeFilesFromDisk[i].SubstringSafe(sub, 30));
					}
					throw new Exception(err.ToString());
                }

                //Assert.AreEqual(expectedOutputStripped[i], haxeFilesFromDisk[i]);
            }
        }

		private static int DifferentAt(string p1, string p2)
		{
			for (int i = 0; i < p1.Length && i < p2.Length; i++)
			{
				if (p1[i] != p2[i])
					return i;
			}

			return Math.Min(p1.Length, p2.Length);
		}
    }
}
