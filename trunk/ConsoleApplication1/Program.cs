using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
	class Program
	{
		static void Main(string[] args)
		{
			var t = new UnitTestProject1.UnitTest1();

			t.NullableTypes();

			//foreach (var m in t.GetType().GetMethods().Where(o => o.GetParameters().Length == 0))				m.Invoke(t, new object[] { });
		}
	}
}
