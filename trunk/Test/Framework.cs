using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Test
{
    public static class TestFramework
    {

        public static void TestCode(string testName, string cSharp, string expectedOutput)
        {
            TestCode(testName, new string[] { cSharp }, new string[] { expectedOutput });
        }
        public static void TestCode(string testName, string cSharp, IEnumerable<string> expectedOutput)
        {
            TestCode(testName, new string[] { cSharp }, expectedOutput);
        }
        public static void TestCode(string testName, IEnumerable<string> cSharp, string expectedOutput)
        {
            TestCode(testName, cSharp, new string[] { expectedOutput });
        }
        public static void TestCode(string testName, IEnumerable<string> cSharp, IEnumerable<string> expectedOutput)
        {
            var cSharpFiles = cSharp.Select(o => new { Path = Path.GetTempFileName() + ".cs", CS = o }).ToList();
            var dir = Path.Combine(@"D:\temp\CS2HX\", testName + @"\src");

            try
            {
                foreach (var f in cSharpFiles)
                    File.WriteAllText(f.Path, f.CS);

                Console.WriteLine("Parsing into " + dir);

                new Cs2hx.Program().Go(cSharpFiles.Select(o => o.Path), dir, new string[] { }, new string[] { });

                Func<string, string> strip = i => Regex.Replace(i, "[\r\n \t]+", " ").Trim();

                var haxeFilesFromDisk = Directory.GetFiles(dir, "*.hx", SearchOption.AllDirectories)
                    .Where(o => Path.GetFileName(o) != "Main.hx" && Path.GetFileName(o) != "Constructors.hx")
                    .Select(o => File.ReadAllText(o))
                    .Select(o => strip(o))
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
                        
                    }
                    Assert.AreEqual(expectedOutputStripped[i], haxeFilesFromDisk[i]);
                }
            }
            finally
            {
                //Delete temp files
                foreach (var f in cSharpFiles)
                {
                    try
                    {
                        File.Delete(f.Path);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
