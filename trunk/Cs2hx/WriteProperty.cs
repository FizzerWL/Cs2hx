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

            bool hasGetter, hasSetter;
            SyntaxNode getterBody, setterBody;
            bool isAutoProperty;

            if (property.AccessorList != null)
            {
                var g = property.AccessorList.Accessors.SingleOrDefault(o => o.Keyword.IsKind(SyntaxKind.GetKeyword));
                hasGetter = g != null;
                getterBody = hasGetter ? g.Body : null;

                var s = property.AccessorList.Accessors.SingleOrDefault(o => o.Keyword.IsKind(SyntaxKind.SetKeyword));
                hasSetter = s != null;
                setterBody = hasSetter ? s.Body : null;

                isAutoProperty = hasGetter && hasSetter && getterBody == null && setterBody == null && !property.Modifiers.Any(SyntaxKind.AbstractKeyword);
            }
            else
            {
                //If AccessorList is null, assume it's an expression bodied member
                hasGetter = true;
                getterBody = property.ExpressionBody.Expression;
                hasSetter = false;
                setterBody = null;
                isAutoProperty = false;
            }


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

                if (hasGetter || isAutoProperty)
                    writer.Write("get");
                else
                    writer.Write("never");

                writer.Write(", ");

                if (hasSetter || isAutoProperty)
                    writer.Write("set");
                else
                    writer.Write("never");

                writer.Write("):");
                writer.Write(TypeProcessor.ConvertType(property.Type));
                writer.Write(";\r\n");
            }



            Action<SyntaxNode, bool> writeRegion = (body, get) =>
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
                    if (isAutoProperty)
                    {
                        if (get)
                            writer.WriteLine("return __autoProp_" + property.Identifier.ValueText + ";");
                        else
                            writer.WriteLine("__autoProp_" + property.Identifier.Value + " = value;");
                    }
                    else if (body == null)
                        throw new Exception("No body at " + Utility.Descriptor(property));
                    else if (body is BlockSyntax)
                    {
                        foreach (var statement in body.As<BlockSyntax>().Statements)
                            Core.Write(writer, statement);
                    }
                    else
                    {
                        //Expression bodied member
                        writer.WriteIndent();
                        writer.Write("return ");
                        Core.Write(writer, body);
                        writer.Write(";\r\n");
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


            if (!isInterface) //interfaces get only the property decl, never the functions
            {
                if (hasGetter || isAutoProperty)
                    writeRegion(getterBody, true);
                if (hasSetter || isAutoProperty)
                    writeRegion(setterBody, false);
            }

            if (isAutoProperty)
                writer.WriteLine("var __autoProp_" + property.Identifier.ValueText + TypeProcessor.ConvertTypeWithColon(property.Type) + " = " + TypeProcessor.DefaultValue(TypeProcessor.ConvertType(property.Type)) + ";");
        }
    }
}
