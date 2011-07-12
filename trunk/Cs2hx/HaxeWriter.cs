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
        private StringBuilder _builder = new StringBuilder(5000);

        public HaxeWriter(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            

            _path = path;
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
            if (_path == null)
                return;

            //Remove read only so we can write it
            if (File.Exists(_path))
                File.SetAttributes(_path, FileAttributes.Normal);

            File.WriteAllText(_path, _builder.ToString());

            //Set read-only on generated files
            File.SetAttributes(_path, FileAttributes.ReadOnly);

            Writer.Dispose();
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
