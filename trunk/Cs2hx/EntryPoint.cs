using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;
using System.Diagnostics;
using Roslyn.Services;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
    public static class EntryPoint
    {
        public static void Main(string[] args)
        {
            try
            {

				Console.WriteLine("C# to haXe Converter\nSee http://www.codeplex.com/cs2hx for full info and documentation.\n\n");

				if (args.Length == 0 || args.Any(o => o == "-?" || o == "--help" || o == "/?"))
				{
					//Print usage
					Console.WriteLine(
@"

Usage:
    cs2hx.exe  /sln:<path to solution file> [options] 


Options available:

	/out:<output directory>
		Directory to write haxe files to.  If not specified, output will be written to the current working directory.

	/config:<configuration>
		The configuration within the passed solution file to use. (Debug, Release, etc.)

	/projects:<comma-delimited list of project names>
		If you don't want to convert all projects in the passed solution, you can provide a list of project names.  Only the projects named here will be converted.

    /extraTranslation:<path to xml file>
        Defines extra conversion parameters for use with this project.  See Translations.xml for examples.");
					return;
				}

				var sourceFiles = new List<string>();
				var outDir = Directory.GetCurrentDirectory();
				var extraTranslations = new List<string>();
				string pathToSolution = null;
				string config = null;
				string projects = null;

				foreach (var arg in args)
				{
					if (arg.StartsWith("/extraTranslation:"))
						extraTranslations.Add(File.ReadAllText(arg.Substring(18)));
					else if (arg.StartsWith("/out:"))
						outDir = arg.Substring(5);
					else if (arg.StartsWith("/sln:"))
						pathToSolution = arg.Substring(5);
					else if (arg.StartsWith("/config:"))
						config = arg.Substring(8);
					else if (arg.StartsWith("/projects:"))
						projects = arg.Substring(10);
					else
						throw new Exception("Invalid argument: " + arg);
				}

				if (pathToSolution == null)
					throw new Exception("/sln parameter not passed");

				var solution = Solution.Load(pathToSolution, config);

				var projectsList = solution.Projects.ToList();

				if (projects != null)
					TrimList(projectsList, projects);

				foreach (var project in projectsList)
				{
					Console.WriteLine("Building project " + project.Name + "...");

					Program.Go((Compilation)project.GetCompilation(), outDir, extraTranslations);
				}

				Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nException:");
                Console.WriteLine(ex);
                Environment.ExitCode = 1;
            }
        }

		private static void TrimList(List<IProject> projectsList, string projectsCsv)
		{
			var split = projectsCsv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

			for (int i = 0; i < projectsList.Count; i++)
			{
				var si = split.IndexOf(projectsList[i].Name);
				if (si != -1)
					split.RemoveAt(si);
				else
				{
					projectsList.RemoveAt(i);
					i--;
				}
			}

			if (split.Count > 0)
				throw new Exception("Project(s) not found: " + string.Join(", ", split));
		}

    }
}
