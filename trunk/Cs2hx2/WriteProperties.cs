using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
    class WriteProperties
    {
        public static void Go(HaxeWriter writer, IEnumerable<PropertyDeclarationSyntax> properties)
        {
            foreach (var property in properties)
            {
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

                    writer.WriteLine();
					writer.WriteOpenBrace();

                    if (property.Modifiers.Any(SyntaxKind.AbstractKeyword))
                    {
                        writer.WriteLine("throw new Exception(\"Abstract item called\");");
                        if (property.Type.ToString() != "void")
                            writer.WriteLine("return " + TypeProcessor.DefaultValue(property.Type) + ";");
                    }
                    else
                    {
						foreach(var statement in region.Body.As<BlockSyntax>().Statements)
							Core.Write(writer, statement);

                        if (!get)
                        {
                            //Unfortunately, all haXe property setters must return a value.
                            writer.WriteLine("return " + TypeProcessor.DefaultValue(property.Type) + ";");
                        }
                    }

					writer.WriteCloseBrace();
					writer.WriteLine();
                };

                var getter = property.AccessorList.Accessors.SingleOrDefault(o => o.Keyword.Kind == SyntaxKind.GetKeyword);
                var setter = property.AccessorList.Accessors.SingleOrDefault(o => o.Keyword.Kind == SyntaxKind.SetKeyword);

                if (getter == null && setter == null)
                    throw new Exception("Property must have either a get or a set");

                if (getter != null && setter != null && setter.Body == null && getter.Body == null)
                {
                    //Both get and set are null, which means this is an automatic property.  This is the equivilant of a field in haxe.
                    WriteFields.WriteField(writer, property.Modifiers, property.Identifier.ValueText, property.Type);
                }
                else
                {


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

                    if (getter != null)
                        writeRegion(getter, true);
                    if (setter != null)
                        writeRegion(setter, false);
                }
            }
        }
    }
}
