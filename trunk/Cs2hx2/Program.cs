using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using System.IO;

namespace Cs2hx
{
    public static class Program
    {

        static public string StandardImports = @"import system.Cs2Hx;
import system.Exception;";

        static internal List<XDocument> TranslationDocs;
        static internal string OutDir;
        //static internal List<MethodDeclarationSyntax> ConvertingExtensionMethods;
        //static internal HashSet<string> EnumNames;
        //static internal Dictionary<string, IEnumerable<DelegateDeclarationSyntax>> Delegates;
        static internal HashSet<string> StaticConstructors = new HashSet<string>();

        static public string[] SystemImports = new[] { 
"system.ArgumentException",
"system.collections.generic.CSDictionary",
"system.collections.generic.HashSet",
"system.collections.generic.KeyValuePair",
"system.DateTime",
"system.diagnostics.Stopwatch",
"system.Enumerable",
"system.Exception",
"system.Guid",
"system.IDisposable",
"system.InvalidOperationException",
"system.io.BinaryReader",
"system.io.BinaryWriter",
"system.KeyNotFoundException",
"system.linq.Linq",
"system.NotImplementedException",
"system.Nullable_Float",
"system.Nullable_Int",
"system.Nullable_Bool",
"system.Nullable_TimeSpan",
"system.Nullable_DateTime",
"system.OverflowException",
"system.RandomAS",
"system.text.StringBuilder",
"system.text.UTF8Encoding",
"system.ThreadAbortException",
"system.TimeoutException",
"system.TimeSpan",
"system.Environment",
"system.xml.linq.XAttribute",
"system.xml.linq.XElement",
"system.xml.linq.XContainer",
"system.xml.linq.XDocument",
"system.xml.linq.XObject",
"haxe.io.Bytes"
        };


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFiles">cs files to parse</param>
        /// <param name="outDir">Folder to write haXe files to. Existing files will be overwritten.</param>
        /// <param name="conditionalCompilationSymbols">Pre-processor conditions to obey. CS2HX is automatically defined.</param>
        /// <param name="extraTranslation">Path to xml files to supplement Translations.xml with.  Users can define their own translations specific to their project here.</param>
        static public void Go(IEnumerable<string> sourceFiles, string outDir, IEnumerable<string> conditionalCompilationSymbols, IEnumerable<string> extraTranslation)
        {

            TranslationDocs = Translations.Translation.BuildTranslationDocs(extraTranslation);

            OutDir = outDir;
            if (!Directory.Exists(OutDir))
                Directory.CreateDirectory(outDir);

            foreach (var sourceFile in sourceFiles)
                if (!File.Exists(sourceFile))
                    throw new FileNotFoundException(sourceFile + " does not exist");


            var parseOptions = new ParseOptions(preprocessorSymbols: conditionalCompilationSymbols);
            var trees = sourceFiles.ToDictionary(o => o, o => SyntaxTree.ParseCompilationUnit(File.ReadAllText(o), options: parseOptions));

            var compilation = Compilation.Create(
                "cs2hxtemp.dll",
                options: new CompilationOptions(assemblyKind: AssemblyKind.DynamicallyLinkedLibrary),
                syntaxTrees: trees.Values,
                references: new[] { new AssemblyFileReference(typeof(object).Assembly.Location) });  //TODO: Add the user's assembly references

            var models = trees.ToDictionary(o => o.Key, o => compilation.GetSemanticModel(o.Value));


            Console.WriteLine("Done parsing.  Generating haXe...");

            var allNamespaces = trees.SelectMany(o => ((CompilationUnitSyntax)o.Value.Root).Members)
                .Cast<NamespaceDeclarationSyntax>()
                .GroupBy(o => o.Name.PlainName)
                .Select(o => new { NamespaceName = o.Key, Namespaces = o })
                .ToList();


            var typesGroupedByNamespace = allNamespaces.Select(o =>
                o.Namespaces.SelectMany(n => n.Members.OfType<TypeDeclarationSyntax>())
                    .GroupBy(t => t.Identifier.ValueText)
                    .Select(t => new { TypeName = t.Key, Partials = t }))
                    .ToList();

            var allTypes = typesGroupedByNamespace.SelectMany(o => o).SelectMany(o => o.Partials);

            //ConvertingExtensionMethods = typesGroupedByNamespace.SelectMany(o => o).SelectMany(o => o.Partials)
            //    .SelectMany(o => o.Members)
            //    .OfType<MethodDeclarationSyntax>()
            //    .Where(o => o.IsExtensionMethod)
            //    .ToList();


            //EnumNames = allTypes.Where(o => o.Type == ClassType.Enum).Select(o => o.Name).ToHashSet(true);

            //Delegates = allNamespaces.SelectMany(o => o.Namespaces).SelectMany(o => o.Children)
            //    .OfType<DelegateDeclaration>()
            //    .Concat(allTypes.SelectMany(o => o.Children).OfType<DelegateDeclaration>())
            //    .GroupBy(o => o.Name)
            //    .ToDictionary(o => o.Key, o => (IEnumerable<DelegateDeclaration>)o);

            foreach (var ns in typesGroupedByNamespace)
                foreach (var type in ns)
                    GenerateType(type.TypeName, type.Partials, t => allNamespaces.Where(o => o.NamespaceName == t).SelectMany(o => o.Namespaces).SelectMany(o => o.Members.OfType<TypeDeclarationSyntax>()));


            Console.WriteLine("Done");
        }

        static private void GenerateType(string typeName, IGrouping<string, TypeDeclarationSyntax> partials, Func<string, IEnumerable<TypeDeclarationSyntax>> getTypesInNamespace)
        {
            var first = partials.First();

            var typeNamespace = first.Parent.As<NamespaceDeclarationSyntax>();

            var dir = Path.Combine(OutDir, typeNamespace.Name.PlainName.Replace(".", Path.DirectorySeparatorChar.ToString())).ToLower();
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var writer = new HaxeWriter(Path.Combine(dir, typeName + ".hx")))
            {
                bool derivesFromObject = true; //TODO

                writer.WriteLine("package " + typeNamespace.Name.PlainName.ToLower() + @";");

                writer.Write("class ");
                writer.Write(typeName);
                writer.Write("\r\n");

                writer.WriteOpenBrace();

                var allChildren = partials.SelectMany(o => o.Members).ToList();

                var fields = allChildren.OfType<FieldDeclarationSyntax>();
                var staticFields = fields.Where(o => o.Modifiers.Any(m => m.ValueText == "static"));
                var staticFieldsNeedingInitialization = staticFields.SelectMany(o => o.Declaration.Variables).Where(o => o.InitializerOpt != null);
                var instanceFieldsNeedingInitialization = fields.Except(staticFields).SelectMany(o => o.Declaration.Variables).Where(o => o.InitializerOpt != null);
                    
                GenerateFields.Go(writer, allChildren.OfType<FieldDeclarationSyntax>());
                writer.WriteLine();
                GenerateProperties.Go(writer, allChildren.OfType<PropertyDeclarationSyntax>());
                writer.WriteLine();
                GenerateMethods.Go(writer, allChildren.OfType<MethodDeclarationSyntax>(), derivesFromObject);

                //if (first.Type != ClassType.Interface)
                {
                    writer.WriteLine();
                    GenerateConstructors.Go(writer, allChildren.OfType<ConstructorDeclarationSyntax>(), derivesFromObject, instanceFieldsNeedingInitialization, staticFieldsNeedingInitialization, typeName);
                }

                writer.WriteCloseBrace();
            }
        }
    }

}
