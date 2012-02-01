using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
    static class GenerateConstructors
    {
        public static void Go(HaxeWriter writer, IEnumerable<ConstructorDeclarationSyntax> constructors, bool derivesFromObject, IEnumerable<VariableDeclaratorSyntax> instanceFieldsNeedingInitialization, IEnumerable<VariableDeclaratorSyntax> staticFieldsNeedingInitialization, string typeName)
        {
            var staticConstructor = constructors.SingleOrDefault(o => o.Modifiers.Any(SyntaxKind.StaticKeyword));

            WriteStaticConstructor(writer, staticConstructor, staticFieldsNeedingInitialization, typeName);

            var normalctors = constructors.Except(staticConstructor);

            if (normalctors.Count() > 1)
                throw new Exception("Overloaded constructors not allowed: " + Utility.Descriptor(normalctors.First()));

            ConstructorDeclarationSyntax ctor;

            if (normalctors.Count() == 0)
                ctor = null;
            else
                ctor = normalctors.Single();

            writer.WriteIndent();

            writer.Write("public function new(");


            if (ctor != null)
            {
                var firstParameter = true;
                foreach (var parameter in ctor.ParameterList.Parameters)
                {
                    if (firstParameter)
                        firstParameter = false;
                    else
                        writer.Write(", ");

                    writer.Write(parameter.Identifier.ValueText);
                    writer.Write(TypeProcessor.TryConvertType(parameter.TypeOpt));
                }
            }

            writer.Write(")\r\n");
            writer.WriteOpenBrace();

            if (!derivesFromObject)
            {
                //TODO
                //if (ctor != null && ctor.Constructor != null)
                //{
                //    switch (ctor.ConstructorInitializer.ConstructorInitializerType)
                //    {
                //        case ConstructorInitializerType.Base:
                //            writer.WriteIndent();
                //            writer.Write("super(");

                //            bool firstArgument = true;
                //            foreach (var arg in ctor.ConstructorInitializer.Arguments)
                //            {
                //                if (firstArgument)
                //                    firstArgument = false;
                //                else
                //                    writer.Write(", ");

                //                WriteStatement(writer, arg);
                //            }

                //            writer.Write(");\r\n");
                //            break;
                //        case ConstructorInitializerType.None:
                //            writer.WriteLine("super();");
                //            break;
                //        case ConstructorInitializerType.This:
                //            throw new Exception(ctor.ConstructorInitializer.ConstructorInitializerType + " not supported.  " + Utility.Descriptor(ctor));
                //    }
                //}
                //else
                    writer.WriteLine("super();");
            }

            foreach (var field in instanceFieldsNeedingInitialization)
            {
                writer.WriteIndent();
                writer.Write(field.Identifier.ValueText);
                writer.Write(" = ");
                Core.WriteStatement(writer, field.InitializerOpt.Value);
                writer.Write(";\r\n");
            }

            if (ctor != null && ctor.BodyOpt != null)
                Core.WriteStatement(writer, ctor.BodyOpt);

            writer.WriteCloseBrace();
        }



        private static void WriteStaticConstructor(HaxeWriter writer, ConstructorDeclarationSyntax staticConstructor, IEnumerable<VariableDeclaratorSyntax> staticInitializationNeeded, string typeName)
        {
            if (staticConstructor == null && staticInitializationNeeded.Count() == 0)
                return; //No static constructor needed

            writer.WriteLine("public static function cctor():Void");
            writer.WriteOpenBrace();

            foreach (var field in staticInitializationNeeded)
            {
                writer.WriteIndent();
                writer.Write(field.Identifier.ValueText);
                writer.Write(" = ");
                Core.WriteStatement(writer, field.InitializerOpt);
                writer.Write(";\r\n");
            }

            if (staticConstructor != null && staticConstructor.BodyOpt != null)
                Core.WriteStatement(writer, staticConstructor.BodyOpt);

            writer.WriteCloseBrace();

            Program.StaticConstructors.Add(typeName);
        }

    }
}
