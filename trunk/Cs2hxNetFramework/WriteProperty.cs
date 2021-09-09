using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cs2hx
{
    class WriteProperty
    {
        public static void Go(HaxeWriter writer, PropertyDeclarationSyntax property)
        {
            var propertySymbol = Program.GetModel(property).GetDeclaredSymbol(property);
            var isInterface = propertySymbol.ContainingType.TypeKind == TypeKind.Interface;
            bool isAutoProperty = false;

            Action<AccessorDeclarationSyntax, bool> writeRegion = (region, get) =>
            {
                writer.WriteIndent();
					
                if (property.Modifiers.Any(SyntaxKind.OverrideKeyword))
                    writer.Write("override ");
                if (property.Modifiers.Any(SyntaxKind.PublicKeyword) || property.Modifiers.Any(SyntaxKind.ProtectedKeyword) || property.Modifiers.Any(SyntaxKind.InternalKeyword))
                    writer.Write("public ");
                if (property.Modifiers.Any(SyntaxKind.PrivateKeyword))
                    writer.Write("private ");
                if (property.Modifiers.Any(SyntaxKind.StaticKeyword))
                    writer.Write("static ");

                writer.Write("function ");
                writer.Write(get ? "get_" : "set_");
                writer.Write(property.Identifier.ValueText);

                string type = TypeProcessor.ConvertType(property.Type);

                if (get)
                    writer.Write("():" + type);
                else
                    writer.Write("(value:" + type + "):" + type);

                var isAbstract = property.Modifiers.Any(SyntaxKind.AbstractKeyword);

                writer.WriteLine();
				writer.WriteOpenBrace();

                if (isAbstract)
                {
                    writer.WriteLine("return throw new Exception(\"Abstract item called\");");
                }
                else
                {
                    if (region.Body == null)
                    {
                        //When we leave the body off in C#, it resolves to an automatic property.
                        isAutoProperty = true;
                        if (get)
                        {
                            writer.WriteLine("return __autoProp_" + property.Identifier.ValueText + ";");
                        }
                        else
                        {
                            writer.WriteLine("__autoProp_" + property.Identifier.Value + " = value;");
                        }
                    }
                    else
                    {
                        foreach (var statement in region.Body.As<BlockSyntax>().Statements)
                            Core.Write(writer, statement);
                    }

                    if (!get)
                    {
                        //all haXe property setters must return a value.
						writer.WriteLine("return value;");
                    }
                }

				writer.WriteCloseBrace();
				writer.WriteLine();
            };
            
            var getter = property.AccessorList.Accessors.SingleOrDefault(o => o.Keyword.Kind() == SyntaxKind.GetKeyword);
            var setter = property.AccessorList.Accessors.SingleOrDefault(o => o.Keyword.Kind() == SyntaxKind.SetKeyword);

            if (getter == null && setter == null)
                throw new Exception("Property must have either a get or a set");


			if (!property.Modifiers.Any(SyntaxKind.OverrideKeyword))
            {
                //Write the property declaration.  Overridden properties don't need this.
                writer.WriteIndent();
				if (property.Modifiers.Any(SyntaxKind.PublicKeyword) || property.Modifiers.Any(SyntaxKind.InternalKeyword))
                    writer.Write("public ");
				if (property.Modifiers.Any(SyntaxKind.StaticKeyword))
                    writer.Write("static ");

                writer.Write("var ");
                writer.Write(property.Identifier.ValueText);
                writer.Write("(");

                if (getter != null)
                    writer.Write("get_" + property.Identifier.ValueText);
                else
                    writer.Write("never");

                writer.Write(", ");

                if (setter != null)
                    writer.Write("set_" + property.Identifier.ValueText);
                else
                    writer.Write("never");

                writer.Write("):");
                writer.Write(TypeProcessor.ConvertType(property.Type));
                writer.Write(";\r\n");
            }

            if (!isInterface) //interfaces get only the property decl, never the functions
            {
                if (getter != null)
                    writeRegion(getter, true);
                if (setter != null)
                    writeRegion(setter, false);
            }

            if (isAutoProperty)
            {
                writer.WriteLine("var __autoProp_" + property.Identifier.ValueText + TypeProcessor.ConvertTypeWithColon(property.Type) + " = " + TypeProcessor.DefaultValue(TypeProcessor.ConvertType(property.Type)) + ";");
            }
        }
    }
}
