using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cs2hx
{
    public class HaxeWriter : IDisposable
    {
        TextWriter Writer;
        private string _path;
        public int Indent;
        private StringBuilder _builder = new StringBuilder(5000);

        public HaxeWriter(string ns, string typeName)
        {
			var typeNamespace = ns.ToLower();

			var dir = Path.Combine(Program.OutDir, typeNamespace.Replace(".", Path.DirectorySeparatorChar.ToString()));
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

            _path = Path.Combine(dir, typeName + ".hx");
            Writer = new StringWriter(_builder);
        }

        public HaxeWriter(TextWriter stream)
        {
            Writer = stream;
        }

        public void WriteLine(string s)
        {
            WriteIndent();
            Writer.WriteLine(s);
        }
        public void WriteLine()
        {
            Writer.WriteLine();
        }

        public void Write(string s)
        {
            Writer.Write(s);
        }

        public void Dispose()
        {
            WriteFinal();

            if (Writer != null)
                Writer.Dispose();
        }

        private void WriteFinal()
        {
            if (_path == null)
                return;

            var final = _builder.ToString();
            if (File.Exists(_path) && File.ReadAllText(_path) == final)
                return; //don't write if it's already up to date.  This just prevents unnecessary reloads by IDEs, it wouldn't cause problems if we didn't check.

            //Remove read only so we can write it
            if (File.Exists(_path))
                File.SetAttributes(_path, FileAttributes.Normal);

            File.WriteAllText(_path, final);

            //Set read-only on generated files
            File.SetAttributes(_path, FileAttributes.ReadOnly);

        }

        public void WriteOpenBrace()
        {
            WriteLine("{");
            Indent++;
        }

        public void WriteCloseBrace()
        {
            Indent--;
            WriteLine("}");
        }

        public void WriteIndent()
        {
            Writer.Write(new string(' ', Indent * 4));
        }

    }
}
