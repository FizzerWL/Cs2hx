using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cs2hx
{
    public class HaxeWriter : IDisposable
    {
        TextWriter Writer;
        private string _path;
        public int Indent;

        public HaxeWriter(string path)
        {
            //Remove read only so we can write it
            if (File.Exists(path))
                File.SetAttributes(path, FileAttributes.Normal);

            _path = path;
            Writer = File.CreateText(path);
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
            Writer.Dispose();

            //Set read-only on generated files
            if (!string.IsNullOrEmpty(_path))
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
