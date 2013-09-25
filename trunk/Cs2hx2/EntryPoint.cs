using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;
using System.Diagnostics;

namespace Cs2hx
{
    public static class EntryPoint
    {
        public static void Main(string[] args)
        {
            try
            {

//				Console.WriteLine("C# to haXe Converter by Randy Ficker.\nSee http://www.codeplex.com/cs2hx for full info and documentation.\n\n");

//				if (args.Length == 0 || args.Any(o => o == "-?" || o == "--help" || o == "/?"))
//				{
//					//Print usage
//					Console.WriteLine(
//@"
//CS2HX can derive your C# source files and pre-processor definitions from a .csproj file or you 
//can specify them yourself.
//
//Usage 1:
//    cs2hx.exe /out:<OutputDirectory> [options] /csproj:<PathToCsprojFile>
//
//Usage 2:
//    cs2hx.exe /out:<OutputDirectory> [options] <SourceFile1> [SourceFile2]...
//
//
//Options available:
//
//    /define:<symbol>            
//        Defines a pre-processor symbol to obey when reading the source file.  The symbol CS2HX is automatically defined for you.
//
//    /extraTranslation:<xml file>
//        Defines extra conversion parameters for use with this project.  See Translations.xml for examples.");
//					return;
//				}

//				var conditionalCompilationSymbols = new List<string>();
//				var sourceFiles = new List<string>();
//				string outDir = Directory.GetCurrentDirectory();
//				var extraTranslations = new List<string>();

//				foreach (var arg in args)
//				{
//					if (arg.StartsWith("/define:"))
//						conditionalCompilationSymbols.Add(arg.Substring(8));
//					else if (arg.StartsWith("/extraTranslation:"))
//						extraTranslations.Add(arg.Substring(18));
//					else if (arg.StartsWith("/out:"))
//						outDir = arg.Substring(5);
//					else if (arg.StartsWith("/csproj:"))
//					{
//						var csProjPath = arg.Substring(8);
//						var csproj = XDocument.Load(csProjPath);
//						var nsMgr = new XmlNamespaceManager(new NameTable());
//						nsMgr.AddNamespace("r", "http://schemas.microsoft.com/developer/msbuild/2003");

//						conditionalCompilationSymbols.AddRange(
//							csproj.XPathSelectElements("/r:Project/r:PropertyGroup/r:DefineConstants", nsMgr).First()
//							.Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(o => o.Trim()));

//						sourceFiles.AddRange(csproj.XPathSelectElements("/r:Project/r:ItemGroup/r:Compile", nsMgr)
//							.Select(o => o.Attribute("Include").Value)
//							.Select(o => Path.Combine(Path.GetDirectoryName(csProjPath), o)));

//						//references.AddRange(csproj.XPathSelectElements("/r:Project/r:ItemGroup/r:Reference").Select(o => GetReferencePath(o, csProjPath)));
//					}
//					else if (arg.StartsWith("/"))
//						throw new Exception("Invalid argument: " + arg);
//					else
//						sourceFiles.Add(arg);
//				}

//				var sw = Stopwatch.StartNew();

//				Program.Go(sourceFiles, outDir, conditionalCompilationSymbols, extraTranslations);

//				Console.WriteLine("Completed in " + sw.Elapsed.TotalSeconds + " seconds");

//				Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nException:");
                Console.WriteLine(ex);
                Environment.ExitCode = 1;
            }
        }

    }
}
