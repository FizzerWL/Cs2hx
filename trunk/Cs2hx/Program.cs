using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.NRefactory.Parser;

namespace Cs2hx
{
    public class Program
    {
        internal string OutDir;
        internal List<MethodDeclaration> ConvertingExtensionMethods;
        static internal List<XDocument> TranslationDocs;
        internal HashSet<string> EnumNames;
        internal Dictionary<string, IEnumerable<DelegateDeclaration>> Delegates;
        internal HashSet<string> StaticConstructors = new HashSet<string>();
        internal int InLambda = 0;
        internal int InForLoop = 0;

        public static string StandardImports = @"import system.Cs2Hx;
import system.Exception;";
        
        public string[] SystemImports = new[] { 
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
"haxe.io.Bytes"
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFiles">cs files to parse</param>
        /// <param name="outDir">Folder to write haXe files to. Existing files will be overwritten.</param>
        /// <param name="conditionalCompilationSymbols">Pre-processor conditions to obey. CS2HX is automatically defined.</param>
        /// <param name="extraTranslation">Path to xml files to supplement Translations.xml with.  Users can define their own translations specific to their project here.</param>
        public void Go(IEnumerable<string> sourceFiles, string outDir, IEnumerable<string> conditionalCompilationSymbols, IEnumerable<string> extraTranslation)
        {

            TranslationDocs = Translations.Translation.BuildTranslationDocs(extraTranslation);

            OutDir = outDir;
            if (!Directory.Exists(OutDir))
                Directory.CreateDirectory(outDir);

            foreach (var sourceFile in sourceFiles)
                if (!File.Exists(sourceFile))
                    throw new FileNotFoundException(sourceFile + " does not exist");

            var parsers = sourceFiles.ToDictionary(o => o, o => ParserFactory.CreateParser(o));

            Console.WriteLine("Parsing...");
            foreach (var parser in parsers.Values)
            {
                parser.Lexer.EvaluateConditionalCompilation = true;
                foreach (var symbol in conditionalCompilationSymbols)
                    parser.Lexer.ConditionalCompilationSymbols.Add(symbol, null);
                if (!conditionalCompilationSymbols.Any(o => o == "CS2HX"))
                    parser.Lexer.ConditionalCompilationSymbols.Add("CS2HX", null);

                parser.Lexer.SkipAllComments = false;

                parser.Parse();
                parser.Dispose(); //Dispose it so it releases the write lock on the source file.
            }

            Console.WriteLine("Done parsing.  Generating haXe...");

            var allNamespaces = parsers.Values
                .SelectMany(o => o.CompilationUnit.Children.OfType<NamespaceDeclaration>())
                .GroupBy(o => o.Name)
                .Select(o => new { NamespaceName = o.Key, Namespaces = o });

            var typesGroupedByNamespace = allNamespaces.Select(o => o.Namespaces.SelectMany(n => n.Children.OfType<TypeDeclaration>())
                .GroupBy(t => t.Name)
                .Select(t => new { TypeName = t.Key, Partials = t }));

            var allTypes = typesGroupedByNamespace.SelectMany(o => o).SelectMany(o => o.Partials);

            ConvertingExtensionMethods = typesGroupedByNamespace.SelectMany(o => o).SelectMany(o => o.Partials)
                .SelectMany(o => o.Children)
                .OfType<MethodDeclaration>()
                .Where(o => o.IsExtensionMethod)
                .ToList();

            EnumNames = allTypes.Where(o => o.Type == ClassType.Enum).Select(o => o.Name).ToHashSet(true);

            Delegates = allNamespaces.SelectMany(o => o.Namespaces).SelectMany(o => o.Children)
                .OfType<DelegateDeclaration>()
                .Concat(allTypes.SelectMany(o => o.Children).OfType<DelegateDeclaration>())
                .GroupBy(o => o.Name)
                .ToDictionary(o => o.Key, o => (IEnumerable<DelegateDeclaration>)o);

            foreach (var ns in typesGroupedByNamespace)
                foreach (var type in ns)
                    GenerateType(type.TypeName, type.Partials, t => allNamespaces.Where(o => o.NamespaceName == t).SelectMany(o => o.Namespaces).SelectMany(o => o.Children.OfType<TypeDeclaration>()));


            GenerateConstructorsHelper(allTypes);
            GenerateMain();
        }

        private void GenerateMain()
        {
            //Create a main if it does not exist.  We won't overwrite an existing main.
            var dest = Path.Combine(OutDir, "Main.hx");

            if (File.Exists(dest))
                return;

            //avoid using the Path constructor of HaxeWriter.  Since this file won't be overwritten, it isn't necessary to mark it read-only, which is our usual way of indicating something is generated.
            using (var writer = new HaxeWriter(File.CreateText(dest)))
            {
                writer.WriteLine(@"/*
Files in this directory were generated by CS2HX.
See http://cs2hx.codeplex.com for more info.

You probably want to call Constructors.init() to initialize the system. This 
calls all static constructors and also ensures that haXe compiles all of
your code.
*/

package ;

import Constructors;

class Main
{
    public static function main():Void
    {
        Constructors.init();

        //Your code here
        trace(""Hello, world!"");
    }
}");
            }
        }

        private void GenerateConstructorsHelper(IEnumerable<TypeDeclaration> allTypes)
        {
            using (var writer = new HaxeWriter(Path.Combine(OutDir, "Constructors.hx")))
            {
                writer.WriteLine(@"/*
This file serves two purposes:  
    1)  It imports every type that CS2HX generated.  haXe will ignore 
        any types that aren't used by haXe code, so this ensures haXe 
        compiles all of your code.

    2)  It lists all the static constructors.  haXe doesn't have the 
        concept of static constructors, so CS2HX generated cctor()
        methods.  You must call these manually.  If you call
        Constructors.init(), all static constructors will be called 
        at once.
*/
package ;");

                foreach (var type in allTypes)
                    writer.WriteLine("import " + type.Parent.As<NamespaceDeclaration>().Name.ToLower() + "." + type.Name + ";");
                writer.WriteLine("import system.TimeSpan;");

                writer.WriteLine("class Constructors");
                writer.WriteOpenBrace();

                writer.WriteLine("public static function init()");
                writer.WriteOpenBrace();
                writer.WriteLine("//Haxe does not support static constructors, so you must call the methods below this comment manually, such as by calling this function.");
                writer.WriteLine("TimeSpan.cctor();");
                foreach (var cctor in StaticConstructors)
                    writer.WriteLine(cctor + ".cctor();");
                writer.WriteCloseBrace();
                writer.WriteCloseBrace();
            }
        }

        private void GenerateType(string typeName, IEnumerable<TypeDeclaration> partials, Func<string, IEnumerable<TypeDeclaration>> getTypesInNamespace)
        {
            var first = partials.First();
            var typeNamespace = first.Parent.As<NamespaceDeclaration>();

            Func<Modifiers, bool> hasModifier = mod => partials.Any(o => o.Modifier.Has(mod));

            var dir = Path.Combine(OutDir, typeNamespace.Name.Replace(".", Path.DirectorySeparatorChar.ToString())).ToLower();
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var writer = new HaxeWriter(Path.Combine(dir, typeName + ".hx")))
            {
                writer.WriteLine("package " + typeNamespace.Name.ToLower() + @";");

                //Write import statements.  First, all StandardImports are always considered
                var imports = SystemImports.ToList();

                //Also allow users to specify extra import statements in the xml file
                foreach (var extraImport in Translations.Translation.ExtraImports())
                    imports.Add(extraImport);

                //Add in imports from the C#'s using statements
                foreach (var usingDeclaration in
                    partials.SelectMany(o => o.Parent.Children.OfType<UsingDeclaration>())
                    .Concat(partials.SelectMany(o => o.Parent.Parent.Children.OfType<UsingDeclaration>()))
                    .SelectMany(o => o.Usings)
                    .Select(o => o.Name)
                    .Distinct()
                    .OrderBy(o => o))
                {
                    if (usingDeclaration.StartsWith("System.") || usingDeclaration == "System")
                        continue; //system usings are handled by our standard imports

                    foreach (var t in getTypesInNamespace(usingDeclaration))
                        imports.Add(usingDeclaration.ToLower() + "." + t.Name);
                }

                //Filter out any ones that aren't being used by this file
                imports = FilterUnusedImports(imports, partials);

                //Cs2hx is always present, since we can't easily determine if it should be filtered
                writer.WriteLine(StandardImports);

                //Write the imports
                foreach (var import in imports.OrderBy(o => o))
                    writer.WriteLine("import " + import + ";");


                bool isEnum = first.Type == ClassType.Enum;
                bool derivesFromObject = true;

                writer.WriteIndent();

                switch (first.Type)
                {
                    case ClassType.Class:
                    case ClassType.Struct:
                    case ClassType.Enum:
                        writer.Write("class ");
                        break;
                    case ClassType.Interface:
                        writer.Write("interface ");
                        break;
                    default:
                        throw new Exception("Need handler for " + first.Type);
                }
                writer.Write(typeName);

                if (!isEnum)
                {
                    //Look for generic arguments
                    var genericArgs = partials.SelectMany(o => o.Templates).ToList();
                    if (genericArgs.Count > 0)
                    {
                        writer.Write("<");
                        writer.Write(string.Join(", ", genericArgs.Select(o => o.Name).ToArray()));
                        writer.Write(">");
                    }


                    writer.Write(" ");

                    var firstExtends = true;
                    foreach (var baseType in partials.SelectMany(o => o.BaseTypes))
                    {
                        //Assume anything that starts with I-Upper is an interface
                        bool isInterface = baseType.Type.StartsWith("I") && char.IsUpper(baseType.Type[1]);

                        if (firstExtends)
                            firstExtends = false;
                        else
                            writer.Write(", ");

                        if (isInterface)
                        {
                            writer.Write("implements ");
                            writer.Write(baseType.Type);

                            if (baseType.GenericTypes.Count > 0)
                            {
                                writer.Write("<");
                                writer.Write(string.Join(", ", baseType.GenericTypes.Select(o => o.Type).ToArray()));
                                writer.Write(">");
                            }
                        }
                        else
                        {
                            writer.Write("extends ");
                            writer.Write(baseType.Type);
                            derivesFromObject = false;
                        }
                    }
                }

                writer.Write("\r\n");

                writer.WriteOpenBrace();

                var allChildren = partials.SelectMany(o => o.Children);

                if (isEnum)
                {
                    GenerateEnumBody(writer, allChildren);
                }
                else
                {
                    var fields = allChildren.OfType<FieldDeclaration>();
                    var staticFields = fields.Where(o => o.Modifier.Has(Modifiers.Static) || o.Modifier.Has(Modifiers.Const));
                    var staticFieldsNeedingInitialization = staticFields.SelectMany(o => o.Fields).Where(o => !o.Initializer.IsNull);
                    var instanceFieldsNeedingInitialization = fields.Except(staticFields).SelectMany(o => o.Fields).Where(o => !o.Initializer.IsNull);
                    
                    GenerateFields(writer, allChildren.OfType<FieldDeclaration>());
                    writer.WriteLine();
                    GenerateProperties(writer, allChildren.OfType<PropertyDeclaration>());
                    writer.WriteLine();
                    GenerateMethods(writer, allChildren.OfType<MethodDeclaration>(), derivesFromObject);

                    if (first.Type != ClassType.Interface)
                    {
                        writer.WriteLine();
                        GenerateConstructors(writer, allChildren.OfType<ConstructorDeclaration>(), derivesFromObject, instanceFieldsNeedingInitialization, staticFieldsNeedingInitialization, typeName);
                    }
                }

                writer.WriteCloseBrace();
            }
        }

        /// <summary>
        /// Filters out import statements that we know aren't needed.
        /// This algorithm isn't perfect, and in some edge cases will leave extra import statements that aren't needed.  These don't cause any problems, though, they just look ugly.
        /// </summary>
        /// <param name="imports"></param>
        /// <param name="partials"></param>
        /// <returns></returns>
        private List<string> FilterUnusedImports(List<string> imports, IEnumerable<TypeDeclaration> partials)
        {
            var allNodes = partials.SelectMany(classType => classType.AllLogicalChildren()).Concat(partials.Cast<INode>());
            var typeObjects = allNodes.SelectMany(o => o.ReferencesTypes());
            var typesReferenced = typeObjects.Select(o => ConvertRawType(o)).RemoveNull().SelectMany(this.SplitGenericTypes).Concat(typeObjects.Select(o => o.Type)).ToHashSet(false);

            var ret = imports.Where(o => typesReferenced.Contains(o.Split('.').Last())).Distinct().ToList();

            return ret;
        }

        static string[] GenericTokens = new string[] { "->", "(", ")", "<", ">", " " };

        private List<string> SplitGenericTypes(string typeString)
        {
            int readerIndex = 0;

            Func<char, bool> isLiteralChar = c => char.IsLetterOrDigit(c) || c == '_';

            Func<string> readToken = () =>
                {
                    var sb = new StringBuilder();
                    var first = typeString[readerIndex++];
                    sb.Append(first.ToString());
                    while (readerIndex < typeString.Length)
                    {
                        var c = typeString[readerIndex];

                        if (isLiteralChar(c) != isLiteralChar(first))
                            return sb.ToString().Trim();

                        sb.Append(c.ToString());
                        readerIndex++;
                    }
                    return sb.ToString().Trim();
                };

            var ret = new List<string>();

            while (readerIndex < typeString.Length)
            {
                var token = readToken();

                if (token.Length == 0)
                    continue;

                if (!GenericTokens.Contains(token))
                    ret.Add(token);
            }

            return ret;
        }

        private void GenerateEnumBody(HaxeWriter writer, IEnumerable<INode> allChildren)
        {
            int lastEnumValue = 0;
            foreach (FieldDeclaration field in allChildren)
            {
                var varDeclaration = field.Fields.Single();

                if (varDeclaration.Initializer.IsNull)
                    lastEnumValue++;
                else
                {
                    bool minus = false;
                    var initializer = varDeclaration.Initializer;

                    if (initializer is UnaryOperatorExpression)
                    {
                        var unary = varDeclaration.Initializer.As<UnaryOperatorExpression>();
                        if (unary.Op == UnaryOperatorType.Minus)
                        {
                            minus = true;
                            initializer = unary.Expression;
                        }
                    }

                    if (!(initializer is PrimitiveExpression))
                        throw new Exception("Enums can only be initalized with constants.");

                    lastEnumValue = (int)initializer.As<PrimitiveExpression>().Value;

                    if (minus)
                        lastEnumValue = -lastEnumValue;
                }

                writer.WriteLine("public static var " + varDeclaration.Name + ":Int = " + lastEnumValue + ";");
            }
        }

        private void GenerateConstructors(HaxeWriter writer, IEnumerable<ConstructorDeclaration> constructors, bool derivesFromObject, IEnumerable<VariableDeclaration> instanceFieldsNeedingInitialization, IEnumerable<VariableDeclaration> staticFieldsNeedingInitialization, string typeName)
        {
            var staticConstructor = constructors.SingleOrDefault(o => o.Modifier.Has(Modifiers.Static));

            WriteStaticConstructor(writer, staticConstructor, staticFieldsNeedingInitialization, typeName);

            var normalctors = constructors.Except(staticConstructor);

            if (normalctors.Count() > 1)
                throw new Exception("Overloaded constructors not allowed: " + Utility.Descriptor(normalctors.First()));

            ConstructorDeclaration ctor;

            if (normalctors.Count() == 0)
                ctor = null;
            else
                ctor = normalctors.Single();

            writer.WriteIndent();

            writer.Write("public function new(");


            if (ctor != null)
            {
                var firstParameter = true;
                foreach (var parameter in ctor.Parameters)
                {
                    if (firstParameter)
                        firstParameter = false;
                    else
                        writer.Write(", ");

                    writer.Write(parameter.ParameterName);
                    writer.Write(TryConvertType(parameter.TypeReference));
                }
            }

            writer.Write(")\r\n");
            writer.WriteOpenBrace();

            if (!derivesFromObject)
            {
                if (ctor != null && ctor.ConstructorInitializer != null)
                {
                    switch (ctor.ConstructorInitializer.ConstructorInitializerType)
                    {
                        case ConstructorInitializerType.Base:
                            writer.WriteIndent();
                            writer.Write("super(");

                            bool firstArgument = true;
                            foreach (var arg in ctor.ConstructorInitializer.Arguments)
                            {
                                if (firstArgument)
                                    firstArgument = false;
                                else
                                    writer.Write(", ");

                                WriteStatement(writer, arg);
                            }

                            writer.Write(");\r\n");
                            break;
                        case ConstructorInitializerType.None:
                            writer.WriteLine("super();");
                            break;
                        case ConstructorInitializerType.This:
                            throw new Exception(ctor.ConstructorInitializer.ConstructorInitializerType + " not supported.  " + Utility.Descriptor(ctor));
                    }
                }
                else
                    writer.WriteLine("super();");
            }

            foreach (var field in instanceFieldsNeedingInitialization)
            {
                writer.WriteIndent();
                writer.Write(field.Name);
                writer.Write(" = ");
                WriteStatement(writer, field.Initializer);
                writer.Write(";\r\n");
            }

            if (ctor != null)
                WriteStatement(writer, ctor.Body);

            writer.WriteCloseBrace();
        }

        private void WriteStaticConstructor(HaxeWriter writer, ConstructorDeclaration staticConstructor, IEnumerable<VariableDeclaration> staticInitializationNeeded, string typeName)
        {
            if (staticConstructor == null && staticInitializationNeeded.Count() == 0)
                return; //No static constructor needed

            writer.WriteLine("public static function cctor():Void");
            writer.WriteOpenBrace();

            foreach (var field in staticInitializationNeeded)
            {
                writer.WriteIndent();
                writer.Write(field.Name);
                writer.Write(" = ");
                WriteStatement(writer, field.Initializer);
                writer.Write(";\r\n");
            }

            if (staticConstructor != null)
                WriteStatement(writer, staticConstructor.Body);

            writer.WriteCloseBrace();

            StaticConstructors.Add(typeName);
        }

        private void GenerateProperties(HaxeWriter writer, IEnumerable<PropertyDeclaration> properties)
        {
            foreach (var property in properties)
            {
                Action<PropertyGetSetRegion, bool> writeRegion = (region, get) =>
                    {
                        Func<Modifiers, bool> hasModifier = mod => region.Modifier.Has(mod) || property.Modifier.Has(mod);

                        writer.WriteIndent();

                        if (hasModifier(Modifiers.Override))
                            writer.Write("override ");
                        if (hasModifier(Modifiers.Public) || hasModifier(Modifiers.Protected) || hasModifier(Modifiers.Internal))
                            writer.Write("public ");
                        if (hasModifier(Modifiers.Private))
                            writer.Write("private ");
                        if (hasModifier(Modifiers.Static))
                            writer.Write("static ");

                        writer.Write("function ");
                        writer.Write(get ? "get_" : "set_");
                        writer.Write(property.Name);

                        string type = TryConvertType(property.TypeReference);

                        if (get)
                            writer.Write("()" + type);
                        else
                            writer.Write("(value" + type + ")" + type);

                        writer.WriteLine();
                        writer.WriteOpenBrace();

                        if (hasModifier(Modifiers.Abstract))
                        {
                            writer.WriteLine("throw new Exception(\"Abstract item called\");");
                            if (property.TypeReference.Type != "System.Void")
                                writer.WriteLine("return " + DefaultValue(property.TypeReference) + ";");
                        }
                        else
                        {
                            WriteStatement(writer, region.Block);

                            if (!get)
                            {
                                //Unfortunately, all haXe property setters must return a value.
                                writer.WriteLine("return " + DefaultValue(property.TypeReference) + ";");
                            }
                        }

                        writer.WriteCloseBrace();
                        writer.WriteLine();
                    };

                if (!property.HasGetRegion && !property.HasSetRegion)
                    throw new Exception("Property must have either a get or a set");

                if (property.HasGetRegion && property.HasSetRegion && property.GetRegion.Block.IsNull && property.SetRegion.Block.IsNull)
                {
                    //Both get and set are null, which means this is an automatic property.  This is the equivilant of a field in haxe.
                    WriteField(writer, property.Modifier, property.Name, property.TypeReference);
                }
                else
                {

                    Func<Modifiers, bool> oneRegionHas = mod => property.Modifier.Has(mod) || (property.HasGetRegion && property.GetRegion.Modifier.Has(mod)) || (property.HasSetRegion && property.SetRegion.Modifier.Has(mod));

                    if (!oneRegionHas(Modifiers.Override))
                    {
                        //Write the property declaration.  Overridden properties don't need this.
                        writer.WriteIndent();
                        if (oneRegionHas(Modifiers.Public) || oneRegionHas(Modifiers.Internal))
                            writer.Write("public ");
                        if (oneRegionHas(Modifiers.Static))
                            writer.Write("static ");
                        writer.Write("var ");
                        writer.Write(property.Name);
                        writer.Write("(");

                        if (property.HasGetRegion)
                            writer.Write("get_" + property.Name);
                        else
                            writer.Write("never");

                        writer.Write(", ");

                        if (property.HasSetRegion)
                            writer.Write("set_" + property.Name);
                        else
                            writer.Write("never");

                        writer.Write(")");
                        writer.Write(TryConvertType(property.TypeReference));
                        writer.Write(";\r\n");
                    }

                    if (property.HasGetRegion)
                        writeRegion(property.GetRegion, true);
                    if (property.HasSetRegion)
                        writeRegion(property.SetRegion, false);
                }
            }
        }

        private void GenerateFields(HaxeWriter writer, IEnumerable<FieldDeclaration> fields)
        {
            foreach (var field in fields)
                foreach (var declaration in field.Fields)
                    WriteField(writer, field.Modifier, declaration.Name, field.TypeReference);
        }

        private void WriteField(HaxeWriter writer, Modifiers modifier, string name, TypeReference type)
        {
            writer.WriteIndent();
            if (modifier.Has(Modifiers.Public) || modifier.Has(Modifiers.Protected) || modifier.Has(Modifiers.Internal))
                writer.Write("public ");
            if (modifier.Has(Modifiers.Private))
                writer.Write("private ");
            if (modifier.Has(Modifiers.Static) || modifier.Has(Modifiers.Const))
                writer.Write("static ");

            writer.Write("var ");

            writer.Write(name);
            writer.Write(TryConvertType(type));
            writer.Write(";");
            writer.WriteLine();
        }

        private void GenerateMethods(HaxeWriter writer, IEnumerable<MethodDeclaration> methods, bool typeDerivesFromObject)
        {
            foreach (var overloadedGroup in methods.GroupBy(o => o.Name))
            {
                //Find the primary method
                var method = overloadedGroup.First(o => o.Parameters.Count == overloadedGroup.Max(m => m.Parameters.Count));

                var defaultParameters = Enumerable.Range(0, method.Parameters.Count).Select(o => (string)null).ToList();

                foreach (var overload in overloadedGroup.Where(o => o != method))
                {
                    Action err = () => { throw new Exception("Overloads must not do anything other than call the primary method: " + Utility.Descriptor(overload)); };
                    //Each overload must resolve to the primary method
                    if (overload.Body.Children.Count > 1)
                        err();
                    var stmt = overload.Body.Children.Single();

                    if (stmt is ReturnStatement)
                        stmt = stmt.As<ReturnStatement>().Expression;
                    else
                    {
                        if (!(stmt is ExpressionStatement))
                            err();
                        stmt = stmt.As<ExpressionStatement>().Expression;
                    }

                    if (!(stmt is InvocationExpression))
                        err();
                    var args = stmt.As<InvocationExpression>().Arguments;
                    if (args.Count != method.Parameters.Count)
                        err();
                    for (int i = 0; i < args.Count; i++)
                    {
                        if (args[i] is PrimitiveExpression)
                            defaultParameters[i] = args[i].As<PrimitiveExpression>().StringValue;
                        else if (!(args[i] is IdentifierExpression))
                            err();
                    }
                }

                writer.WriteIndent();

                if ((method.Name != "ToString" || !typeDerivesFromObject) && method.Modifier.Has(Modifiers.Override))
                    writer.Write("override ");
                if (method.Modifier.Has(Modifiers.Public) || method.Modifier.Has(Modifiers.Protected) || method.Modifier.Has(Modifiers.Internal))
                    writer.Write("public ");
                if (method.Modifier.Has(Modifiers.Private))
                    writer.Write("private ");
                if (method.Modifier.Has(Modifiers.Static))
                    writer.Write("static ");

                writer.Write("function ");
                writer.Write(method.Name == "ToString" ? "toString" : method.Name);

                if (method.Templates.Count > 0)
                {
                    writer.Write("<");
                    writer.Write(string.Join(", ", method.Templates.Select(o => o.Name).ToArray()));
                    writer.Write(">");
                }

                writer.Write("(");

                var parameterNumber = 0;
                foreach (var parameter in method.Parameters)
                {
                    if (parameter.ParamModifier.Has(ParameterModifiers.Out))
                        throw new Exception("out is not supported: " + Utility.Descriptor(method));
                    if (parameter.ParamModifier.Has(ParameterModifiers.Ref))
                        throw new Exception("ref is not supported: " + Utility.Descriptor(method));

                    if (parameterNumber > 0)
                        writer.Write(", ");

                    writer.Write(parameter.ParameterName);
                    writer.Write(TryConvertType(parameter.TypeReference));

                    var def = defaultParameters[parameterNumber];
                    if (def != null)
                        writer.Write(" = " + def);

                    parameterNumber++;
                }

                writer.Write(")");
                writer.Write(TryConvertType(method.TypeReference));

                if (method.Modifier.Has(Modifiers.Abstract))
                {
                    writer.WriteLine();
                    writer.WriteOpenBrace();
                    writer.WriteLine("throw new Exception(\"Abstract item called\");");
                    if (method.TypeReference.Type != "System.Void")
                        writer.WriteLine("return " + DefaultValue(method.TypeReference) + ";");
                    writer.WriteCloseBrace();
                }
                else if (method.Body.IsNull)
                    writer.Write(";\r\n"); //interface methods
                else
                {
                    writer.WriteLine();
                    writer.WriteOpenBrace();
                    foreach (var statement in method.Body.Children)
                        WriteStatement(writer, statement);
                    writer.WriteCloseBrace();
                }
            }
        }


        private void WriteStatement(HaxeWriter writer, INode statement, bool suppressSemicolonAndIndent = false)
        {
            if (statement is ExpressionStatement)
                WriteExpressionStatement(writer, statement.As<ExpressionStatement>(), suppressSemicolonAndIndent);
            else if (statement is IdentifierExpression)
                writer.Write(statement.As<IdentifierExpression>().Identifier);
            else if (statement is PrimitiveExpression)
                WritePrimitiveExpression(writer, statement.As<PrimitiveExpression>());
            else if (statement is LocalVariableDeclaration)
                WriteLocalVariableDeclaration(writer, statement.As<LocalVariableDeclaration>(), suppressSemicolonAndIndent);
            else if (statement is IfElseStatement)
                WriteIfElseStatement(writer, statement.As<IfElseStatement>());
            else if (statement is BinaryOperatorExpression)
                WriteBinaryOperatorExpression(writer, statement.As<BinaryOperatorExpression>());
            else if (statement is InvocationExpression)
                WriteInvocationExpression(writer, statement.As<InvocationExpression>());
            else if (statement is MemberReferenceExpression)
                WriteMemberReferenceExpression(writer, statement.As<MemberReferenceExpression>());
            else if (statement is UnaryOperatorExpression)
                WriteUnaryOperatorExpression(writer, statement.As<UnaryOperatorExpression>());
            else if (statement is ReturnStatement)
                WriteReturnStatement(writer, statement.As<ReturnStatement>());
            else if (statement is AssignmentExpression)
                WriteAssignmentExpression(writer, statement.As<AssignmentExpression>());
            else if (statement is ArrayCreateExpression)
                WriteArrayCreateExpression(writer, statement.As<ArrayCreateExpression>());
            else if (statement is ForeachStatement)
                WriteForeachStatement(writer, statement.As<ForeachStatement>());
            else if (statement is BlockStatement)
                WriteBlockStatement(writer, statement.As<BlockStatement>());
            else if (statement is IndexerExpression)
                WriteIndexerExpression(writer, statement.As<IndexerExpression>());
            else if (statement is DoLoopStatement)
                WriteDoLoopStatement(writer, statement.As<DoLoopStatement>());
            else if (statement is BreakStatement)
                WriteBreakStatement(writer, statement.As<BreakStatement>());
            else if (statement is ForStatement)
                WriteForStatement(writer, statement.As<ForStatement>());
            else if (statement is LambdaExpression)
                WriteLambdaExpression(writer, statement.As<LambdaExpression>());
            else if (statement is CastExpression)
                WriteCastExpression(writer, statement.As<CastExpression>());
            else if (statement is ParenthesizedExpression)
                WriteParenthesizedExpression(writer, statement.As<ParenthesizedExpression>());
            else if (statement is ObjectCreateExpression)
                WriteObjectCreateExpression(writer, statement.As<ObjectCreateExpression>());
            else if (statement is CheckedStatement)
                WriteCheckedStatement(writer, statement.As<CheckedStatement>());
            else if (statement is UsingStatement)
                WriteUsingStatement(writer, statement.As<UsingStatement>());
            else if (statement is ThrowStatement)
                WriteThrowStatement(writer, statement.As<ThrowStatement>());
            else if (statement is TryCatchStatement)
                WriteTryCatchStatement(writer, statement.As<TryCatchStatement>());
            else if (statement is BaseReferenceExpression)
                WriteBaseReferenceExpression(writer, statement.As<BaseReferenceExpression>());
            else if (statement is TypeOfIsExpression)
                WriteTypeOfIsExpression(writer, statement.As<TypeOfIsExpression>());
            else if (statement is ConditionalExpression)
                WriteConditionalExpression(writer, statement.As<ConditionalExpression>());
            else if (statement is ThisReferenceExpression)
                WriteThisReferenceExpression(writer, statement.As<ThisReferenceExpression>());
            else if (statement is SwitchStatement)
                WriteSwitchStatement(writer, statement.As<SwitchStatement>());
            else if (statement is ContinueStatement)
                WriteContinueStatement(writer, statement.As<ContinueStatement>());
            else if (statement is TypeOfExpression)
                WriteTypeOfExpression(writer, statement.As<TypeOfExpression>());
            else if (statement is LockStatement)
                WriteLockStatement(writer, statement.As<LockStatement>());
            else if (statement is EmptyStatement)
            {
                //Eat empty statements
            }
            else if (statement is DirectionExpression || statement is YieldStatement)
                throw new ArgumentException(statement.GetType().Name + " is not supported.  Found in " + Utility.Descriptor(statement));

            else
                throw new Exception("Need handler for " + statement.GetType().Name + " at " + Utility.Descriptor(statement));
        }

        private void WriteLockStatement(HaxeWriter writer, LockStatement lockStatement)
        {
            //Eat lock statements - ActionScript is unfortunately single-threaded.
            WriteStatement(writer, lockStatement.EmbeddedStatement);
        }

        private void WriteTypeOfExpression(HaxeWriter writer, TypeOfExpression typeOfExpression)
        {
            writer.Write("typeof(");
            writer.Write(typeOfExpression.TypeReference.Type);
            writer.Write(")");
        }

        private void WriteContinueStatement(HaxeWriter writer, ContinueStatement continueStatement)
        {
            if (InForLoop > 0)
                throw new Exception("Cannot use \"continue\" in a \"for\" loop.  Consider changing to a while loop instead. " + Utility.Descriptor(continueStatement));
            writer.WriteLine("continue;");
        }

        private void WriteThisReferenceExpression(HaxeWriter writer, ThisReferenceExpression thisReferenceExpression)
        {
            if (InLambda > 0)
                throw new InvalidOperationException("Cannot use \"this\" in a lambda. You must create a reference to the \"this\" object outside the lambda for use inside. Found at " + Utility.Descriptor(thisReferenceExpression));
            writer.Write("this");
        }

        private void WriteSwitchStatement(HaxeWriter writer, SwitchStatement switchStatement)
        {
            writer.WriteIndent();
            writer.Write("switch (");
            WriteStatement(writer, switchStatement.SwitchExpression);
            writer.Write(")\r\n");
            writer.WriteOpenBrace();
            foreach (var section in switchStatement.SwitchSections)
            {
                foreach (var label in section.SwitchLabels)
                {
                    writer.WriteIndent();

                    if (label.IsDefault)
                        writer.Write("default");
                    else
                    {
                        writer.Write("case ");
                        WriteStatement(writer, label.Label);
                    }

                    writer.Write(":\r\n");
                }
                writer.Indent++;

                var write = section.Children;

                if (write.Count == 0)
                    throw new Exception("haXe does not support fall-through case statements. " + Utility.Descriptor(section));

                foreach (var child in write.Where(o => !(o is BreakStatement))) 
                    WriteStatement(writer, child);

                writer.Indent--;
            }
            writer.WriteCloseBrace();
        }

        private void WriteConditionalExpression(HaxeWriter writer, ConditionalExpression conditionalExpression)
        {
            WriteStatement(writer, conditionalExpression.Condition);
            writer.Write(" ? ");
            WriteStatement(writer, conditionalExpression.TrueExpression);
            writer.Write(" : ");
            WriteStatement(writer, conditionalExpression.FalseExpression);
        }

        private void WriteTypeOfIsExpression(HaxeWriter writer, TypeOfIsExpression typeOfIsExpression)
        {
            var destType = ConvertRawType(typeOfIsExpression.TypeReference, false, true);
            if (destType == null)
                destType = "Dynamic";

            writer.Write("Std.is(");
            WriteStatement(writer, typeOfIsExpression.Expression);
            writer.Write(", ");
            writer.Write(destType);
            writer.Write(")");

        }

        private void WriteBaseReferenceExpression(HaxeWriter writer, BaseReferenceExpression baseReferenceExpression)
        {
            writer.Write("super");
        }

        private void WriteTryCatchStatement(HaxeWriter writer, TryCatchStatement tryCatchStatement)
        {
            writer.WriteLine("try");
            writer.WriteOpenBrace();
            WriteStatement(writer, tryCatchStatement.StatementBlock);
            writer.WriteCloseBrace();

            foreach (var catchClause in tryCatchStatement.CatchClauses)
            {
                string varName = catchClause.VariableName;

                if (string.IsNullOrEmpty(varName))
                    varName = "noVarName_" + Guid.NewGuid().ToString("N");

                writer.WriteIndent();
                writer.Write("catch (");
                writer.Write(varName);
                writer.Write(TryConvertType(catchClause.TypeReference));
                writer.Write(")\r\n");
                writer.WriteOpenBrace();
                WriteStatement(writer, catchClause.StatementBlock);
                writer.WriteCloseBrace();
            }

            if (!tryCatchStatement.FinallyBlock.IsNull)
                throw new Exception("Finally blocks are not supported in haxe. " + Utility.Descriptor(tryCatchStatement));
        }

        private void WriteThrowStatement(HaxeWriter writer, ThrowStatement throwStatement)
        {
            writer.WriteIndent();
            writer.Write("throw");
            if (!throwStatement.Expression.IsNull)
            {
                writer.Write(" ");
                WriteStatement(writer, throwStatement.Expression);
            }
            writer.Write(";\r\n");
        }

        private void WriteUsingStatement(HaxeWriter writer, UsingStatement usingStatement)
        {
            INode resourceAcquisition = usingStatement.ResourceAcquisition;
            if (resourceAcquisition is ExpressionStatement)
                resourceAcquisition = resourceAcquisition.As<ExpressionStatement>().Expression;

            //Ensure the using statement is a local variable - we can't deal with things we can't reliably repeat in the finally block
            if (!(resourceAcquisition is IdentifierExpression))
                throw new Exception("Using statements must reference a local variable. " + Utility.Descriptor(usingStatement));

            var resource = resourceAcquisition.As<IdentifierExpression>().Identifier;

            if (Utility.RecurseAllChildren(usingStatement.EmbeddedStatement).OfType<ReturnStatement>().Any())
                throw new Exception("You cannot return from within a using block. " + Utility.Descriptor(usingStatement));

            writer.WriteIndent();
            writer.WriteLine("var __disposed_" + resource + ":Bool = false;");
            writer.WriteLine("try");
            writer.WriteOpenBrace();
            WriteStatement(writer, usingStatement.EmbeddedStatement);
            writer.WriteLine("__disposed_" + resource + " = true;");
            writer.WriteLine(resource + ".Dispose();");
            writer.WriteCloseBrace();

            writer.WriteLine("catch (__catch_" + resource + ":Dynamic)");
            writer.WriteOpenBrace();
            writer.WriteLine("if (!__disposed_" + resource + ")");
            writer.WriteLine("    " + resource + ".Dispose();");
            writer.WriteLine("throw __catch_" + resource + ";");
            writer.WriteCloseBrace();
        }

        private void WriteCheckedStatement(HaxeWriter writer, CheckedStatement checkedStatement)
        {
            WriteStatement(writer, checkedStatement.Block);
        }

        private void WriteObjectCreateExpression(HaxeWriter writer, ObjectCreateExpression objectCreateExpression)
        {
            if (!objectCreateExpression.ObjectInitializer.IsNull)
                throw new Exception("C# 3.5 object initialization syntax is not supported. " + Utility.Descriptor(objectCreateExpression));

            var translate = Translations.Translation.GetTranslation(Translations.Translation.TranslationType.Method, ".ctor", objectCreateExpression.CreateType) as Translations.Method;

            var type = ConvertRawType(objectCreateExpression.CreateType);

            if (type == null)
                throw new Exception("New'd Type not found");

            writer.Write("new ");
            writer.Write(type);
            writer.Write("(");

            var firstParameter = true;
            foreach (var parameter in TranslateParameters(translate, objectCreateExpression.Parameters, null))
            {
                if (firstParameter)
                    firstParameter = false;
                else
                    writer.Write(", ");

                WriteStatement(writer, parameter);
            }

            writer.Write(")");
        }

        private IEnumerable<Expression> TranslateParameters(Translations.Translation translate, List<Expression> list, InvocationExpression invoke)
        {
            if (translate == null)
                return list;
            else if (translate is Translations.Method)
                return translate.As<Translations.Method>().TranslateParameters(list, invoke, this);
            else
                throw new Exception("Need handler for " + translate.GetType().Name);
        }


        private void WriteParenthesizedExpression(HaxeWriter writer, ParenthesizedExpression parenthesizedExpression)
        {
            writer.Write("(");
            WriteStatement(writer, parenthesizedExpression.Expression);
            writer.Write(")");
        }

        private void WriteCastExpression(HaxeWriter writer, CastExpression castExpression)
        {
            var destType = this.ConvertRawType(castExpression.CastTo) ?? "Dynamic";

            switch (castExpression.CastType)
            {
                case CastType.Cast:

                    string castingFrom = null;
                    {
                        TypeReference type = null;
                        if (castExpression.Expression is IdentifierExpression && Utility.TryFindType(castExpression.Expression.As<IdentifierExpression>(), out type))
                            castingFrom = ConvertRawType(type);
                    }

                    if (destType == "Int" && castingFrom != null && castingFrom == "Int")
                    {
                        //Just eat casts from Int to Int.  Enums getting casted to int fall here, and since we use ints to represent enums anyway, it's not necessary
                        WriteStatement(writer, castExpression.Expression);
                    }
                    else if (destType == "Int")
                    {
                        writer.Write("Std.int(");
                        WriteStatement(writer, castExpression.Expression);
                        writer.Write(")");
                    }
                    else if (destType == "Float")
                    {
                        WriteStatement(writer, castExpression.Expression);
                    }
                    else if (castingFrom != null && castingFrom == "Dynamic")
                        WriteStatement(writer, castExpression.Expression); //ignore casts from dynamic as dynamic can be used as any type.  haXe throws errors when casting dynamic too, which is odd.
                    else
                    {
                        writer.Write("cast(");
                        WriteStatement(writer, castExpression.Expression);
                        writer.Write(", ");
                        writer.Write(destType);
                        writer.Write(")");
                    }
                    break;
                case CastType.TryCast:
                    throw new Exception("\"as\" keyword is not supported. " + Utility.Descriptor(castExpression));
                default:
                    throw new Exception("Need handler for " + castExpression.CastType);
            }
        }

        private void WriteLambdaExpression(HaxeWriter writer, LambdaExpression lambdaExpression)
        {
            string returnType = "";
            string[] parameterTypes = lambdaExpression.Parameters.Select(o => "").ToArray();

            if (lambdaExpression.Parent.Parent is LocalVariableDeclaration)
            {
                var lambdaType = lambdaExpression.Parent.Parent.As<LocalVariableDeclaration>().TypeReference;

                if (lambdaType.Type == "Action")
                {
                    returnType = ":Void";
                    parameterTypes = lambdaType.GenericTypes.Select(this.TryConvertType).ToArray();
                }
                else if (lambdaType.Type == "Func")
                {
                    returnType = TryConvertType(lambdaType.GenericTypes.Last());
                    parameterTypes = lambdaType.GenericTypes.Take(lambdaType.GenericTypes.Count - 1).Select(this.TryConvertType).ToArray();
                }
            }

            writer.Write("function (");

            int paramNum = 0;
            foreach (var param in lambdaExpression.Parameters)
            {
                if (paramNum > 0)
                    writer.Write(", ");

                writer.Write(param.ParameterName);
                writer.Write(parameterTypes[paramNum]);

                paramNum++;
            }

            writer.Write(")");
            writer.Write(returnType);
            writer.Write("\r\n");
            writer.WriteOpenBrace();

            InLambda++;

            if (!lambdaExpression.ExpressionBody.IsNull)
            {
                writer.WriteIndent();

                if (returnType != ":Void")
                    writer.Write("return ");

                WriteStatement(writer, lambdaExpression.ExpressionBody);
                writer.Write(";\r\n");
            }
            else if (!lambdaExpression.StatementBody.IsNull)
                WriteStatement(writer, lambdaExpression.StatementBody);
            else
                throw new Exception("No lambda body found");

            InLambda--;

            writer.WriteCloseBrace();
            writer.WriteIndent();
        }

        private void WriteBreakStatement(HaxeWriter writer, BreakStatement breakStatement)
        {
            writer.WriteLine("break;");
        }

        private void WriteForStatement(HaxeWriter writer, ForStatement forStatement)
        {
            InForLoop++;

            writer.WriteLine("{ //for");
            writer.Indent++;


            foreach (var init in forStatement.Initializers)
                WriteStatement(writer, init, false);

            writer.WriteIndent();
            writer.Write("while (");

            WriteStatement(writer, forStatement.Condition);
            writer.Write(")\r\n");
            writer.WriteOpenBrace();
            WriteStatement(writer, forStatement.EmbeddedStatement);

            foreach (var iterator in forStatement.Iterator)
                WriteStatement(writer, iterator, false);

            writer.WriteCloseBrace();
            writer.Indent--;
            writer.WriteLine("} //end for");

            InForLoop--;
        }

        private void WriteDoLoopStatement(HaxeWriter writer, DoLoopStatement doLoopStatement)
        {
            if (doLoopStatement.ConditionPosition == ConditionPosition.Start)
            {
                writer.WriteIndent();
                writer.Write("while (");
                WriteStatement(writer, doLoopStatement.Condition);
                writer.Write(")\r\n");
                writer.WriteOpenBrace();
                WriteStatement(writer, doLoopStatement.EmbeddedStatement);
                writer.WriteCloseBrace();
            }
            else
            {
                writer.WriteLine("do");
                writer.WriteOpenBrace();
                WriteStatement(writer, doLoopStatement.EmbeddedStatement);
                writer.WriteCloseBrace();
                writer.WriteIndent();
                writer.Write("while (");
                WriteStatement(writer, doLoopStatement.Condition);
                writer.Write(");\r\n");
            }
        }

        private void WriteIndexerExpression(HaxeWriter writer, IndexerExpression indexerExpression)
        {
            if (indexerExpression.Indexes.Count > 1)
                throw new Exception("Multiple indexers?");

            var expression = indexerExpression.Indexes.Single();
            WriteStatement(writer, indexerExpression.TargetObject);

            if (indexerExpression.TargetObject is IdentifierExpression)
            {
                TypeReference typeRef;
                if (Utility.TryFindType(indexerExpression.TargetObject.As<IdentifierExpression>(), out typeRef))
                {
                    if (typeRef.Type == "Dictionary")
                    {
                        writer.Write(".GetValue(");
                        WriteStatement(writer, expression);
                        writer.Write(")");
                        return;
                    }
                }
            }

            writer.Write("[");
            WriteStatement(writer, expression);
            writer.Write("]");
        }

        private void WriteBlockStatement(HaxeWriter writer, BlockStatement blockStatement)
        {
            foreach (var child in blockStatement.Children)
                WriteStatement(writer, child);
        }

        private void WriteForeachStatement(HaxeWriter writer, ForeachStatement foreachStatement)
        {
            writer.WriteIndent();
            writer.Write("for (");
            writer.Write(foreachStatement.VariableName);
            writer.Write(" in ");
            WriteStatement(writer, foreachStatement.Expression);

            TypeReference typeRef;
            if (foreachStatement.Expression is IdentifierExpression && Utility.TryFindType(foreachStatement.Expression.As<IdentifierExpression>(), out typeRef) && typeRef.Type == "HashSet")
                writer.Write(".Values()");

            writer.Write(")\r\n");
            writer.WriteOpenBrace();
            WriteStatement(writer, foreachStatement.EmbeddedStatement);
            writer.WriteCloseBrace();
        }

        private void WriteArrayCreateExpression(HaxeWriter writer, ArrayCreateExpression arrayCreateExpression)
        {
            if (arrayCreateExpression.CreateType.Type == "System.Byte")
            {
                if (arrayCreateExpression.ArrayInitializer.CreateExpressions.Count > 0)
                    throw new Exception("Cannot use array initialization syntax for byte arrays");

                writer.Write("Bytes.alloc(");

				if (arrayCreateExpression.Arguments.Any())
					WriteStatement(writer, arrayCreateExpression.Arguments.Single(), false);
				writer.Write(")");
            }
            else
            {
                writer.Write("[ ");

                bool firstItem = true;
                foreach (var arrayItem in arrayCreateExpression.ArrayInitializer.CreateExpressions)
                {
                    if (firstItem)
                        firstItem = false;
                    else
                        writer.Write(", ");

                    WriteStatement(writer, arrayItem);
                }

                writer.Write(" ]");
            }
        }

        private void WriteReturnStatement(HaxeWriter writer, ReturnStatement returnStatement)
        {
            writer.WriteIndent();
            writer.Write("return");

            if (!returnStatement.Expression.IsNull)
            {
                writer.Write(" ");
                WriteStatement(writer, returnStatement.Expression);
            }
            writer.Write(";\r\n");
        }

        private void WriteUnaryOperatorExpression(HaxeWriter writer, UnaryOperatorExpression unaryOperatorExpression)
        {
            switch (unaryOperatorExpression.Op)
            {
                case UnaryOperatorType.Not: writer.Write("!"); break;
                case UnaryOperatorType.Decrement: writer.Write("--"); break;
                case UnaryOperatorType.Increment: writer.Write("++"); break;
                case UnaryOperatorType.Minus: writer.Write("-"); break;
                case UnaryOperatorType.Plus: writer.Write("+"); break;
                case UnaryOperatorType.BitNot: writer.Write("~"); break;

                case UnaryOperatorType.Dereference:
                case UnaryOperatorType.AddressOf:
                    throw new Exception("Not supported: " + unaryOperatorExpression.Op + " found in " + Utility.Descriptor(unaryOperatorExpression));
            }

            WriteStatement(writer, unaryOperatorExpression.Expression);

            switch (unaryOperatorExpression.Op)
            {
                case UnaryOperatorType.PostDecrement: writer.Write("--"); break;
                case UnaryOperatorType.PostIncrement: writer.Write("++"); break;
            }
        }

        public bool IsExtensionMethod(string methodName, Expression expression, out string methodNamespace)
        {
            var ext = ConvertingExtensionMethods.FirstOrDefault(o => o.Name == methodName);
            if (ext != null)
            {
                methodNamespace = ext.Parent.Parent.As<NamespaceDeclaration>().Name.ToLower() + "." + ext.Parent.As<TypeDeclaration>().Name;
                return true;
            }

            methodNamespace = null;
            return false;
        }


        private void WriteBinaryOperatorExpression(HaxeWriter writer, BinaryOperatorExpression binaryOperatorExpression)
        {
            Func<string> opString = () =>
                {
                    switch (binaryOperatorExpression.Op)
                    {
                        case BinaryOperatorType.Add: return "+";
                        case BinaryOperatorType.Equality: return "==";
                        case BinaryOperatorType.Concat: return "+";
                        case BinaryOperatorType.LessThan: return "<";
                        case BinaryOperatorType.GreaterThan: return ">";
                        case BinaryOperatorType.LessThanOrEqual: return "<=";
                        case BinaryOperatorType.GreaterThanOrEqual: return ">=";
                        case BinaryOperatorType.InEquality: return "!=";
                        case BinaryOperatorType.Subtract: return "-";
                        case BinaryOperatorType.LogicalAnd: return "&&";
                        case BinaryOperatorType.LogicalOr: return "||";
                        case BinaryOperatorType.Modulus: return "%";
                        case BinaryOperatorType.BitwiseOr: return "|";
                        case BinaryOperatorType.ShiftLeft: return "<<";
                        case BinaryOperatorType.ShiftRight: return ">>";
                        case BinaryOperatorType.Multiply: return "*";
                        case BinaryOperatorType.Divide: return "/";
                        case BinaryOperatorType.BitwiseAnd: return "&";
                        default:
                            throw new Exception("Need handler for " + binaryOperatorExpression.Op + " at " + Utility.Descriptor(binaryOperatorExpression));
                    }
                };

            WriteStatement(writer, binaryOperatorExpression.Left);
            writer.Write(" ");
            writer.Write(opString());
            writer.Write(" ");
            WriteStatement(writer, binaryOperatorExpression.Right);
        }

        private void WriteIfElseStatement(HaxeWriter writer, IfElseStatement ifElseStatement)
        {
            writer.WriteIndent();
            writer.Write("if (");
            WriteStatement(writer, ifElseStatement.Condition);
            writer.Write(")\r\n");
            writer.WriteOpenBrace();

            foreach (var statement in ifElseStatement.TrueStatement)
                WriteStatement(writer, statement);

            writer.WriteCloseBrace();

            if (ifElseStatement.HasElseIfSections)
                foreach (var elseIf in ifElseStatement.ElseIfSections)
                {
                    writer.WriteIndent();
                    writer.Write("else if (");
                    WriteStatement(writer, elseIf.Condition);
                    writer.Write(")\r\n");
                    writer.WriteOpenBrace();

                    WriteStatement(writer, elseIf.EmbeddedStatement);

                    writer.WriteCloseBrace();
                }

            if (ifElseStatement.HasElseStatements)
            {
                writer.WriteLine("else");
                writer.WriteOpenBrace();

                foreach (var statement in ifElseStatement.FalseStatement)
                    WriteStatement(writer, statement);

                writer.WriteCloseBrace();
            }
        }

        /// <summary>
        /// Returns a bogus value that fits the passed type.  This can be used after a throw statement, for example, to satisify the compiler.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string DefaultValue(TypeReference type)
        {
            var t = ConvertRawType(type);

            if (t == "Int" || t == "Float")
                return "0";
            else if (t == "Bool")
                return "false";
            else
                return "null";
        }

        /// <summary>
        /// Attempts to convert the passed TypeReference to a haXe type. If the type is not known, an empty string will be returned and type inferance will be used.  If the type is known, the type will be returned preceeded by a colon
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string TryConvertType(TypeReference type)
        {
            var ret = ConvertRawType(type);
            if (ret == null)
                return string.Empty;
            else
                return ":" + ret;
        }

        public string ConvertRawType(TypeReference type)
        {
            return ConvertRawType(type, false, false);
        }

        private static IEnumerable<NamedArgumentExpression> GetAttributes(INode node)
        {
            if (!(node is AttributedNode))
                return new List<NamedArgumentExpression>();

            var attributedNode = (AttributedNode)node;
            return attributedNode.Attributes.SelectMany(o => o.Attributes).Where(o => o.Name == "Cs2Hx").SelectMany(o => o.NamedArguments);
        }

        public string ConvertRawType(TypeReference type, bool ignoreArrayType, bool ignoreGenericArguments)
        {
            //Check for the Cs2Hx attribute which could have a directive that tells us what type to use
            var attributeReplace = GetAttributes(type.Parent).SingleOrDefault(o => o.Name == "ReplaceWithType");
            if (attributeReplace != null)
                return attributeReplace.Expression.As<PrimitiveExpression>().Value.ToString();

            if (type.IsArrayType && type.Type == "System.Byte")
                return "Bytes";
            if (!ignoreArrayType && type.IsArrayType)
                return "Array<" + ConvertRawType(type, true, false) + ">";

            var translation = Translations.Translation.GetTranslation(Translations.Translation.TranslationType.Type, type.Type, type) as Translations.Type;

            string genericSuffix;
            if (type.GenericTypes.Count > 0 && !ignoreGenericArguments)
                genericSuffix = "<" + string.Join(", ", type.GenericTypes.Select(o => ConvertRawType(o)).ToArray()) + ">";
            else
                genericSuffix = string.Empty;

            if (translation != null)
                return translation.ReplaceWith + genericSuffix;

            if (Delegates.ContainsKey(type.Type))
            {
                var dlgs = Delegates[type.Type];
                DelegateDeclaration dlg;

                if (dlgs.Count() == 1)
                    dlg = dlgs.Single();
                else
                {
                    dlg = dlgs.FirstOrDefault(o => o.Templates.Count == type.GenericTypes.Count);
                    if (dlg == null)
                        throw new Exception("Delegate type could not be uniquely identified: " + type.ToString());
                }

                Func<TypeReference, string> convertDelegateParameter = t =>
                    {
                        var template = dlg.Templates.SingleOrDefault(o => o.Name == t.Type);

                        if (template == null)
                            return ConvertRawType(t);
                        else
                        {
                            int templatePosition = dlg.Templates.IndexOf(template);
                            return ConvertRawType(type.GenericTypes[templatePosition]);
                        }
                    };

                if (dlg.Parameters.Count == 0)
                    return "(Void -> " + convertDelegateParameter(dlg.ReturnType) + ")";
                else
                    return "(" + string.Join(" -> ", dlg.Parameters.Select(o => convertDelegateParameter(o.TypeReference)).Concat(convertDelegateParameter(dlg.ReturnType)).ToArray()) + ")";
            }

            if (type.Type == "Action")
            {
                if (type.GenericTypes.Count == 0)
                    return "(Void -> Void)";
                else
                    return "(" + string.Join(" -> ", type.GenericTypes.Select(ConvertRawType).ToArray()) + " -> Void)";
            }

            if (type.Type == "Func")
            {
                if (type.GenericTypes.Count == 1)
                    return "(Void -> " + ConvertRawType(type.GenericTypes.Single()) + ")";
                else
                    return "(" + string.Join(" -> ", type.GenericTypes.Select(ConvertRawType).ToArray()) + ")";
            }

            //Handle generic types that we can convert
            switch (type.Type)
            {
                case "IEnumerable":
                    //return "Iterable" + genericSuffix; I'd love to use Iterable here, but ActionScript can't enumerate on an iterable when using haxe with -as3.    Array also gives a lot more perf since Iterable gets converted to * when using -as3.
                    return "Array" + genericSuffix;
                case "LinkedList":
                    return "List" + genericSuffix;
                case "Queue":
                case "List":
                case "IList":
                case "Stack":
                    return "Array" + genericSuffix;
                case "HashSet":
                    return "HashSet" + genericSuffix;
                case "Dictionary":
                    return "CSDictionary" + genericSuffix;
                case "KeyValuePair":
                    return "KeyValuePair" + genericSuffix;
                case "System.Nullable":
                case "Nullable":
                    return "Nullable_" + ConvertRawType(type.GenericTypes.Single());
            }

            //All enums are represented as ints.
            if (EnumNames.Contains(type.Type))
                return "Int";

            //Handle non-generic types
            switch (type.Type)
            {
                case "System.Void": return "Void";
                case "Boolean":
                case "System.Boolean": return "Bool";
                case "System.Object": return "Dynamic";

                case "System.Single":
                case "System.Double":
                case "System.UInt64":
                case "System.Int64":
                case "Int64":
                case "UInt64":
                case "Single":
                case "Double":
                    return "Float";

                case "System.String":
                    return "String";

                case "System.Char":
                case "System.UInt32":
                case "System.UInt16":
                case "System.Int16":
                case "System.Byte":
                case "System.Int32":
                case "Int32":
                case "Byte":
                case "Int16":
                case "UInt16":
                case "Char":
                    return "Int";

                case "var": return null;
                default:

                    //This type does not get translated and gets used as-is
                    return type.Type + genericSuffix;
            }
        }

        private void WriteLocalVariableDeclaration(HaxeWriter writer, LocalVariableDeclaration localVariableDeclaration, bool suppressSemicolon)
        {
            string type = TryConvertType(Utility.DetermineType(localVariableDeclaration));
            
            foreach(var variable in localVariableDeclaration.Variables)
            {
                if (!suppressSemicolon)
                    writer.WriteIndent();

                writer.Write("var ");
                writer.Write(variable.Name);
                writer.Write(type);

                if (!variable.Initializer.IsNull)
                {
                    writer.Write(" = ");
                    WriteStatement(writer, variable.Initializer);
                }

                if (!suppressSemicolon)
                    writer.Write(";\r\n");
            }
        }

        private void WritePrimitiveExpression(HaxeWriter writer, PrimitiveExpression primitiveExpression)
        {
            switch (primitiveExpression.LiteralFormat)
            {
                case LiteralFormat.VerbatimStringLiteral:
                    var raw = primitiveExpression.StringValue;

                    if (!raw.StartsWith("@"))
                        throw new Exception("Expected Verbatim to start with @");

                    writer.Write(raw.Substring(1).Replace("\\", "\\\\").Replace("\"\"", "\\\""));
                    break;
                default:
                    if (primitiveExpression.StringValue.StartsWith("'") && primitiveExpression.StringValue.EndsWith("'"))
                    {
                        Action error = () => { throw new Exception("Unexpected single quote string: " + primitiveExpression.StringValue + " at " + Utility.Descriptor(primitiveExpression)); };
                        var quote = primitiveExpression.StringValue.Trim('\'');

                        if (quote.Length == 2)
                        {
                            if (quote[0] != '\\')
                                error();

                            quote = quote.Substring(1);
                        }
                        else if (quote.Length != 1)
                            error();

                        writer.Write(((int)quote[0]).ToString());

                    }
                    else if (primitiveExpression.StringValue.EndsWith("f") && !primitiveExpression.StringValue.StartsWith("0x"))
                        writer.Write(primitiveExpression.StringValue.Substring(0, primitiveExpression.StringValue.Length - 1));
                    else
                        writer.Write(primitiveExpression.StringValue);
                    break;
            }            
        }

        private void WriteExpressionStatement(HaxeWriter writer, ExpressionStatement expressionStatement, bool suppressSemicolonAndIndent)
        {
            if (!suppressSemicolonAndIndent)
                writer.WriteIndent();

            WriteStatement(writer, expressionStatement.Expression);

            if (!suppressSemicolonAndIndent)
                writer.Write(";\r\n");
        }

        private void WriteAssignmentExpression(HaxeWriter writer, AssignmentExpression assignmentExpression)
        {
            Func<string> determineOperator = () =>
                {
                    switch (assignmentExpression.Op)
                    {
                        case AssignmentOperatorType.Assign: return "=";
                        case AssignmentOperatorType.Add: return "+=";
                        case AssignmentOperatorType.ConcatString: return "+=";
                        case AssignmentOperatorType.Subtract: return "-=";
                        case AssignmentOperatorType.Modulus: return "%=";
                        case AssignmentOperatorType.Divide: return "/=";
                        case AssignmentOperatorType.DivideInteger: return "/=";
                        case AssignmentOperatorType.Multiply: return "*=";
                        case AssignmentOperatorType.BitwiseOr: return "|=";
                        default:
                            throw new Exception("Need handler for " + assignmentExpression.Op.ToString());
                    }
                };

            WriteStatement(writer, assignmentExpression.Left);
            writer.Write(" ");
            writer.Write(determineOperator());
            writer.Write(" ");
            WriteStatement(writer, assignmentExpression.Right);
        }

        private void WriteMemberReferenceExpression(HaxeWriter writer, MemberReferenceExpression memberReferenceExpression)
        {
            string memberName = memberReferenceExpression.MemberName;

            var translation = Translations.Translation.GetTranslation(Translations.Translation.TranslationType.Property, memberName, memberReferenceExpression) as Translations.Property;

            if (translation != null)
                memberName = translation.ReplaceWith;

            if (memberReferenceExpression.TargetObject is TypeReferenceExpression && (memberName == "MaxValue" || memberName == "MinValue" || memberName == "Empty"))
            {
                //Support int.MaxValue/int.MaxValue/etc
                var typeRef = memberReferenceExpression.TargetObject.As<TypeReferenceExpression>();

                if (memberName == "Empty" && typeRef.TypeReference.Type == "System.String")
                    writer.Write("\"\"");
                else if (memberName == "MinValue" && typeRef.TypeReference.Type == "System.Double")
                    writer.Write("-1.7976931348623e+308");  //We change double.MinValue since haXe can't deal with the real MinValue.  Any checks against this should use <= in place of ==
                else if (memberName == "MaxValue" && typeRef.TypeReference.Type == "System.Int64")
                    writer.Write("999900000000000000"); //We change long.MaxValue since haXe can't deal with the real MaxValue. Any checks against this should use >= in place of ==
                else
                    writer.Write(Type.GetType(typeRef.TypeReference.Type).GetField(memberName).GetValue(null).ToString());
            }
            else
            {
                WriteStatement(writer, memberReferenceExpression.TargetObject);
                writer.Write(".");
                writer.Write(memberName);
            }
        }

        private void WriteInvocationExpression(HaxeWriter writer, InvocationExpression invocationExpression)
        {
            bool isExtensionMethod;
            Translations.Method translate = null;

            if (!(invocationExpression.TargetObject is MemberReferenceExpression))
            {
                WriteStatement(writer, invocationExpression.TargetObject);
                isExtensionMethod = false;
            }
            else
            {
                var memberReferenceExpression = invocationExpression.TargetObject.As<MemberReferenceExpression>();

                translate = Translations.Translation.GetTranslation(Translations.Translation.TranslationType.Method, memberReferenceExpression.MemberName, memberReferenceExpression.TargetObject) as Translations.Method;
                string methodName;
                string extensionNamespace;

                if (translate == null)
                    methodName = memberReferenceExpression.MemberName;
                else
                    methodName = translate.ReplaceWith ?? memberReferenceExpression.MemberName;

                if (translate != null && translate.IsExtensionMethod)
                {
                    isExtensionMethod = true;
                    extensionNamespace = translate.ExtensionNamespace;
                }
                else if (IsExtensionMethod(memberReferenceExpression.MemberName, memberReferenceExpression.TargetObject, out extensionNamespace))
                    isExtensionMethod = true;
                else
                    isExtensionMethod = false;

                if (methodName.StartsWith("**"))
                {
                    switch (methodName.Substring(2))
                    {
                        case "CompileError": throw new Exception("Error: Method " + memberReferenceExpression.MemberName + " not allowed.  " + Utility.Descriptor(invocationExpression));
                        default:
                            throw new Exception("Need handler for " + methodName);
                    }
                }

                if (isExtensionMethod)
                {
                    writer.Write(extensionNamespace);
                    writer.Write(".");
                    writer.Write(methodName);

                    if (translate == null || !translate.SkipExtensionParameter)
                    {
                        //We must try writing the statement.  If it ends up being the same as extensionNamespace, that means this extension method was called in a non-extension way and we should omit the extension parameter.
                        var extensionParameterSb = new StringBuilder();
                        using (var extensionParameterWriter = new HaxeWriter(new StringWriter(extensionParameterSb)))
                            WriteStatement(extensionParameterWriter, memberReferenceExpression.TargetObject);

                        if (extensionNamespace.ToLower().EndsWith(extensionParameterSb.ToString().ToLower()))
                            isExtensionMethod = false;
                        else
                        {
                            writer.Write("(");
                            writer.Write(extensionParameterSb.ToString());
                        }
                    }
                    else
                        isExtensionMethod = false;
                }
                else
                {
                    if (memberReferenceExpression.TargetObject is TypeReferenceExpression)
                    {
                        switch (methodName)
                        {
                            case "Parse":
                                var t = ConvertRawType(memberReferenceExpression.TargetObject.As<TypeReferenceExpression>().TypeReference);
                                if (t == null)
                                    throw new Exception("Could not identify Parse method at " + Utility.Descriptor(memberReferenceExpression));
                                if (t == "Bool")
                                {
                                    writer.Write("HaxeUtility.ParseBool");
                                }
                                else if (t == "Int" || t == "Float")
                                {
                                    writer.Write("Std.parse" + t);
                                }
                                else
                                    throw new Exception("Parse method on " + t + " is not supported.  " + Utility.Descriptor(memberReferenceExpression));
                                
                                break;
                            case "IsNaN":
                                writer.Write("Math.isNaN");
                                break;
                            case "IsInfinity":
                                writer.Write("Cs2Hx.IsInfinity");
                                break;
                            default:
                                throw new Exception(methodName + " is not supported.  " + Utility.Descriptor(memberReferenceExpression));
                        }
                    }
                    else
                    {
                        string varType;
                        TypeReference type;
                        if (memberReferenceExpression.TargetObject is IdentifierExpression && Utility.TryFindType(memberReferenceExpression.TargetObject.As<IdentifierExpression>(), out type))
                            varType = ConvertRawType(type);
                        else
                            varType = null;

                        if (methodName.Equals("ToString", StringComparison.OrdinalIgnoreCase) && (varType == "Int" || varType == "Float"))
                        {
                            //ToString()'s on primitive types get replaced with Std.string
                            writer.Write("Std.string(");
                            WriteStatement(writer, memberReferenceExpression.TargetObject);
                            writer.Write(")");

                            if (invocationExpression.Arguments.Count > 0)
                                throw new Exception("Primitive type's ToString detected with parameters.  These are not supported in haXe. " + Utility.Descriptor(invocationExpression));

                            return; //Skip any parameters
                        }
                        else if (methodName == "sort" && invocationExpression.Arguments.Count == 0)
                        {
                            //Sorts without parameters need to get the default sort function added
                            WriteStatement(writer, memberReferenceExpression.TargetObject);
                            writer.Write(".");

                            switch (varType)
                            {
                                case "Array<Int>":
                                    writer.Write("sort(Cs2Hx.SortInts)");
                                    break;
                                case "Array<Float>":
                                    writer.Write("sort(Cs2Hx.SortFloats)");
                                    break;
                                default:
                                    throw new Exception("Unknown default sort type: " + varType + ".  " + Utility.Descriptor(invocationExpression));
                            }

                            return;
                        }
                        else if (methodName == "split" && varType == "String" && invocationExpression.Arguments.Count == 1 && invocationExpression.Arguments.Single() is PrimitiveExpression)
                        {
                            //C# split takes a char, but haXe split takes a string.
                            WriteStatement(writer, memberReferenceExpression.TargetObject);
                            writer.Write(".split(");
                            writer.Write(invocationExpression.Arguments.Single().As<PrimitiveExpression>().StringValue);
                            writer.Write(")");
                            return;
                        }
                        else if (methodName == "As" && memberReferenceExpression.TypeArguments.Count == 1)
                        {
                            var castTo = this.ConvertRawType(memberReferenceExpression.TypeArguments.Single()) ?? "Dynamic";

                            writer.Write("cast(");
                            WriteStatement(writer, memberReferenceExpression.TargetObject);
                            writer.Write(", ");
                            writer.Write(castTo);
                            writer.Write(")");
                            return;
                        }
                        else
                        {
                            WriteStatement(writer, memberReferenceExpression.TargetObject);
                            writer.Write(".");
                            writer.Write(methodName);
                        }
                    }
                }
            }

            if (!isExtensionMethod)
                writer.Write("(");

            var firstArg = !isExtensionMethod;
            foreach (var arg in TranslateParameters(translate, invocationExpression.Arguments, invocationExpression))
            {
                if (firstArg)
                    firstArg = false;
                else
                    writer.Write(", ");

                WriteStatement(writer, arg);
            }


            writer.Write(")");
        }
    }
}
