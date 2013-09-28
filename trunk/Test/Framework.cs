using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Roslyn.Compilers;

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
            var dir = Path.Combine(@"D:\temp\CS2HX\", testName + @"\src");

            Console.WriteLine("Parsing into " + dir);

			var compilation = Compilation.Create(testName, new CompilationOptions(OutputKind.DynamicallyLinkedLibrary)) //dll so we don't require a main method
				.AddReferences(MetadataReference.CreateAssemblyReference("mscorlib"))
				.AddReferences(MetadataReference.CreateAssemblyReference("System"))
				.AddReferences(MetadataReference.CreateAssemblyReference("System.Core"))
				.AddSyntaxTrees(cSharp.Select(o => SyntaxTree.ParseText(o)));

            Cs2hx.Program.Go(compilation, dir, extraTranslation);

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
                    Console.WriteLine("---------------Expected----------------");
                    Console.WriteLine(expectedOutputStripped[i]);
                    Console.WriteLine("---------------Actual----------------");
                    Console.WriteLine(haxeFilesFromDisk[i]);


					Console.WriteLine("Different at " + DifferentAt(expectedOutputStripped[i], haxeFilesFromDisk[i]));
                }
                Assert.AreEqual(expectedOutputStripped[i], haxeFilesFromDisk[i]);
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
