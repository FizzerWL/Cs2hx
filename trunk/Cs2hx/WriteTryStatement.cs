using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteTryStatement
	{
		public static void Go(HaxeWriter writer, TryStatementSyntax tryStatement)
		{
			if (tryStatement.Finally != null)
				throw new Exception("Finally blocks are not supported in haxe. " + Utility.Descriptor(tryStatement.Finally));

			writer.WriteLine("try");
			Core.Write(writer, tryStatement.Block);

			foreach (var catchClause in tryStatement.Catches)
			{
				if (Program.DoNotWrite.ContainsKey(catchClause))
					continue;

				writer.WriteIndent();
				writer.Write("catch (");

				if (catchClause.Declaration == null)
					writer.Write("__ex:Dynamic");
				else
				{
					writer.Write(string.IsNullOrWhiteSpace(catchClause.Declaration.Identifier.ValueText) ? "__ex" : catchClause.Declaration.Identifier.ValueText);

                    var type = TypeProcessor.ConvertTypeWithColon(catchClause.Declaration.Type);

                    if (type == ":system.Exception")
                        writer.Write(":Dynamic"); //when the C# code catches Exception, we assume they want to catch everything, and in haxe we do that by catching Dynamic
                    else
					    writer.Write(type);
				}
				writer.Write(")\r\n");
				Core.Write(writer, catchClause.Block);
			}

		}
	}
}
