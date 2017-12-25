using System;
using System.Collections.Generic;
using System.Reflection;
using Cs2hx;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test;

namespace UnitTestProject1
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void HelloWorld()
		{

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            Console.WriteLine(""Hello, World!"");
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        system.Console.WriteLine(""Hello, World!"");
    }
    public function new()
    {
    }
}");
		}
		[TestMethod]
		public void IfStatement()
		{

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            string notInitialized;
            int myNum = 0;
            notInitialized = ""InitMe!"";

            if (myNum > 4)
                myNum = 2;
            else if (notInitialized == ""asdf"")
                myNum = 1;
            else
                myNum = 999;

            Console.WriteLine(myNum == 999 ? ""One"" : ""Two"");
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"
class Utilities
{
    public static function SomeFunction():Void
    {
        var notInitialized:String;
        var myNum:Int = 0;
        notInitialized = ""InitMe!"";

        if (myNum > 4)
        {
            myNum = 2;
        }
        else if (notInitialized == ""asdf"")
        {
            myNum = 1;
        }
        else
        {
            myNum = 999;
        }

        system.Console.WriteLine(myNum == 999 ? ""One"" : ""Two"");
    }
    public function new()
    {
    }
}");
		}

		[TestMethod]
		public void Loops()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            while (true)
            {
                Console.WriteLine(""hi"");
                break;
            }
			
			while (true)
				Console.WriteLine(""nobreak"");

            for (int i=0;i<50;i++)
                Console.WriteLine(i);

            do
            {
                Console.WriteLine(""Dowhile"");
            }
            while (false);
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        while (true)
        {
            system.Console.WriteLine(""hi"");
            break;
        }

		while (true)
		{
			system.Console.WriteLine(""nobreak"");
		}

        { //for
            var i:Int = 0;
            while (i < 50)
            {
                system.Console.WriteLine_Int32(i);
                i++;
            }
        } //end for
        do
        {
            system.Console.WriteLine(""Dowhile"");
        }
        while (false);
    }
    public function new()
    {
    }
}");
		}


		[TestMethod]
		public void EnumerateOnString()
		{

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.Linq;

namespace Blargh
{
    public class Foo
    {
        public Foo()
        {
            var s = ""hello"";
			var chars = s.ToCharArray();
			foreach(var c in s)
			{
			}
			s.Select(o => o);
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Foo
{
    public function new()
    {
        var s:String = ""hello"";
		var chars:Array<Int> = system.Cs2Hx.ToCharArray(s);
		for (c in Cs2Hx.ToCharArray(s))
		{
		}
		system.linq.Enumerable.Select(Cs2Hx.ToCharArray(s), function (o:Int):Int { return o; } );
    }
}");
		}

		[TestMethod]
		public void Indexing()
		{

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.Collections.Generic;

namespace Blargh
{
    public class Foo
    {
        public Foo()
        {
            var dict = new Dictionary<int, int>();
			dict[3] = 4;
			var i = dict[3];
			var array = new int[3];
			array[0] = 1;
			array[1]++;
			var str = ""hello"";
			var c = str[2];
			var list = new List<int>();
			i = list[0];
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Foo
{
    public function new()
    {
        var dict:system.collections.generic.Dictionary<Int, Int> = new system.collections.generic.Dictionary<Int, Int>();
		dict.SetValue_TKey(3, 4);
		var i:Int = dict.GetValue_TKey(3);
		var array:Array<Int> = [ ];
		array[0] = 1;
		array[1]++;
		var str:String = ""hello"";
		var c:Int = str.charCodeAt(2);
		var list:Array<Int> = new Array<Int>();
		i = list[0];
    }
}");
		}

		[TestMethod]
		[ExpectedException(typeof(AggregateException), "Events are not supported")]
		public void Events()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.Text;

namespace Blargh
{

    public class Foo
    {
		public static event Action Evt;
		public static event Action<int> EvtArg;
		public event Action NonStatic;

		public Foo()
		{
			Evt += Program_Evt;
			Evt -= Program_Evt;
			Evt();
			EvtArg += Program_EvtArg;
			EvtArg -= Program_EvtArg;
			EvtArg(3);
	
		}

		static void Program_Evt()
		{
		}
		static void Program_EvtArg(int i)
		{
		}
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Foo
{
	public static var Evt:CsEvent<(Void -> Void)>;
	public static var EvtArg:CsEvent<(Int -> Void)>;
	public var NonStatic:CsEvent<(Void -> Void)>;
	
    public function new()
    {
		NonStatic = new CsEvent<(Void -> Void)>();
		Evt.Add(Program_Evt);
		Evt.Remove(Program_Evt);
		Evt.Invoke0();
		EvtArg.Add(Program_EvtArg);
		EvtArg.Remove(Program_EvtArg);
		EvtArg.Invoke1(3);
    }

	static function Program_Evt():Void
	{
	}

	static function Program_EvtArg(i:Int):Void
	{
	}

	public static function cctor():Void
	{
		Evt = new CsEvent<(Void -> Void)>();
		EvtArg = new CsEvent<(Int -> Void)>();
	}
}");
		}
		[TestMethod]
		public void NamedParameters()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.Text;

namespace Blargh
{

    public class Foo
    {
		public void Bar(int a, int b, int c, int d = 3)
		{
		}

        public void Bar2(int a, int b = 1, int c = 2, int d = 4)
        {
        }

        public Foo()
		{
			Bar(1,2,3,4);
			Bar(1,2,3);
			Bar(a: 1, b: 2, c: 3, d: 4);
			Bar(a: 1, b: 2, c: 3);
			Bar(a: 1, c: 3, b: 2);
			Bar(1, c: 3, b: 2);
			Bar(1, 2, c: 3, d: 4);
            Bar2(1, d: 5);
		}
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Foo
{
	public function Bar(a:Int, b:Int, c:Int, d:Int = 3):Void
	{
	}
    public function Bar2(a:Int, b:Int = 1, c:Int = 2, d:Int = 4):Void
	{
	}
    public function new()
    {
		Bar(1, 2, 3, 4);
		Bar(1, 2, 3);
		Bar(1, 2, 3, 4);
		Bar(1, 2, 3);
		Bar(1, 2, 3);
		Bar(1, 2, 3);
		Bar(1, 2, 3, 4);
        Bar2(1, 1, 2, 5);
    }
}");
		}

		[TestMethod]
		public void NestedClasses()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.Text;

namespace Blargh
{

    public class Outer
    {
		public class Inner
		{
			public int InnerField;
			public Inner()
			{
				InnerField = 0;
			}
		}

		public Outer()
		{
			var i = new Inner();
			i.InnerField = 4;
		}
    }
}", new[] { @"
package blargh;
" + WriteImports.StandardImports + @"

class Outer
{
    public function new()
    {
		var i:blargh.Outer_Inner = new blargh.Outer_Inner();
		i.InnerField = 4;
    }
}",
  @"
package blargh;
" + WriteImports.StandardImports + @"

class Outer_Inner
{
	public var InnerField:Int;
    public function new()
    {
		InnerField = 0;
		InnerField = 0;
    }
}"
  
  });

            //We have to live with generating two InnerField = 0 lines.  One is directly copied verbatim from the source, and the other is generated by our code that ensures that all fields are initialized.
		}

		[TestMethod]
		public void AnonymousTypes()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.Text;

namespace Blargh
{

    public class Foo
    {
        public Foo()
		{
			var i = new { Field1 = 3, Field2 = new StringBuilder() };
			Console.WriteLine(i.Field1);
		}
    }
}", new[] { @"
package blargh;
" + WriteImports.StandardImports + @"

class Foo
{
    public function new()
    {
		var i:Anon_Field1_Int__Field2_system_text_StringBuilder = new Anon_Field1_Int__Field2_system_text_StringBuilder(3, new system.text.StringBuilder());
		system.Console.WriteLine_Int32(i.Field1);
    }
}",
 @"
package anonymoustypes;
" + WriteImports.StandardImports + @"

class Anon_Field1_Int__Field2_system_text_StringBuilder
{
	public var Field1:Int;
	public var Field2:system.text.StringBuilder;

    public function new(Field1:Int, Field2:system.text.StringBuilder)
    {
		this.Field1 = Field1;
		this.Field2 = Field2;
    }
}"

  
  });
		}

		[TestMethod]
		public void AttributesAreIgnored()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using Shared;

#if !CS2HX
namespace Shared
{
	public class TestingAttribute : Attribute
    {
    }
}
#endif

namespace Blargh
{

    public class Foo
    {
        [Testing]
        public string Str;
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Foo
{
    public var Str:String;
    public function new()
    {
    }
}");
		}


		[TestMethod]
		public void PreprocessorDirectives()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public class SomeClass
    {
#if CS2HX
		public static var Variable:Int;
#endif

        public SomeClass()
        {
#if CS2HX
			Console.WriteLine(""cs2hx1"");
#else
			Console.WriteLine(""not1"");
#endif
#if CS2HX //comment
			Console.WriteLine(""cs2hx2"");
#else

			Console.WriteLine(""not2"");
#if nope
			Console.WriteLine(""not3"");
#endif

#endif
			Console.WriteLine(""outside"");

#if CS2HX
			Console.WriteLine(""cs2hx3"");
#endif
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class SomeClass
{
	public static var Variable:Int;

    public function new()
    {
		Console.WriteLine(""cs2hx1"");
		Console.WriteLine(""cs2hx2"");
		system.Console.WriteLine(""outside"");
		Console.WriteLine(""cs2hx3"");
    }
}");
		}


		[TestMethod]
		public void OfType()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System.Text;
using System.Linq;

namespace Blargh
{
    public class SomeClass
    {
        public SomeClass()
        {
            var a = new[] { 1,2,3 };
			var b = a.OfType<StringBuilder>().ToList();
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class SomeClass
{
    public function new()
    {
        var a:Array<Int> = [ 1, 2, 3 ];
		var b:Array<system.text.StringBuilder> = system.linq.Enumerable.ToList(system.linq.Enumerable.OfType(a, system.text.StringBuilder));
    }
}");
		}

		[TestMethod]
		public void GlobalKeyword()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public class SomeClass
    {
        public SomeClass()
        {
            global::Blargh.SomeClass c = null;
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class SomeClass
{
    public function new()
    {
        var c:blargh.SomeClass = null;
    }
}");
		}


		[TestMethod]
		public void DefaultParameter()
		{

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public class SomeClass
    {
        public void Foo(int i1, int i2 = 4, string s1 = ""hi"")
        {
        }

        public SomeClass(int i3 = 9)
        {
            Foo(4);
            Foo(5, 6);
            Foo(6, 7, ""eight"");
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class SomeClass
{
    public function Foo(i1:Int, i2:Int = 4, s1:String = ""hi""):Void
    {
    }

    public function new(i3:Int = 9)
    {
        Foo(4);
        Foo(5, 6);
        Foo(6, 7, ""eight"");
    }
}");
		}

		[TestMethod]
		public void ForStatementWithNoCondition()
		{

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            for(;;)
            {
                Console.WriteLine(""Hello, World!"");
            }
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        { //for
            while (true)
            {
                system.Console.WriteLine(""Hello, World!"");
            }
        } //end for
    }
    public function new()
    {
    }
}");
		}

		[TestMethod]
		public void AutomaticProperties()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    class Box
    {
        public float Width
        {
            get;
            set;
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Box
{
    public var Width(get_Width, set_Width):Float;
    public function get_Width():Float
    {
        return __autoProp_Width;
    }
    public function set_Width(value:Float):Float
    {
        __autoProp_Width = value;
        return value;
    }
    var __autoProp_Width:Float;
    public function new()
    {
    }
}");
		}

		[TestMethod]
		public void GenericClass()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.Collections.Generic;

namespace Blargh
{
	public class KeyValueList<K, V> : IEquatable<K>
	{
		private List<KeyValuePair<K, V>> _list = new List<KeyValuePair<K, V>>();

		public void Add(K key, V value)
		{
			this._list.Add(new KeyValuePair<K, V>(key, value));
		}

		public void Insert(int index, K key, V value)
		{
			_list.Insert(index, new KeyValuePair<K, V>(key, value));
		}

		public void Clear()
		{
			_list.Clear();
			var castTest = (K)MemberwiseClone();
		}

		public void RemoveAt(int index)
		{
			_list.RemoveAt(index);
		}

		public bool Equals(K other)
		{
			throw new NotImplementedException();
		}
	}
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class KeyValueList<K, V> implements system.IEquatable<K>
{
    private var _list:Array<system.collections.generic.KeyValuePair<K, V>>;

    public function Add(key:K, value:V):Void
    {
        this._list.push(new system.collections.generic.KeyValuePair<K, V>(key, value));
    }
    public function Insert(index:Int, key:K, value:V):Void
    {
        _list.insert(index, new system.collections.generic.KeyValuePair<K, V>(key, value));
    }
    public function Clear():Void
    {
		system.Cs2Hx.Clear(_list);
        var castTest:K = MemberwiseClone();
    }
    public function RemoveAt(index:Int):Void
    {
        _list.splice(index, 1);
    }
	public function Equals(other:K):Bool
	{
		return throw new system.NotImplementedException();
	}
    public function new()
    {
        _list = new Array<system.collections.generic.KeyValuePair<K, V>>();
    }
}");
		}

		[TestMethod]
		public void ByteArrays()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            byte[] b1 = new byte[4];
			byte[] b2 = new byte[Foo()];
        }
		static int Foo() { return 4; }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        var b1:haxe.io.Bytes = haxe.io.Bytes.alloc(4);
		var b2:haxe.io.Bytes = haxe.io.Bytes.alloc(Foo());
    }
	static function Foo():Int
	{
		return 4;
	}
    public function new()
    {
    }
}");
		}

		[TestMethod]
		[ExpectedException(typeof(AggregateException), "Cannot use \"continue\" in a \"for\" loop")]
		public void CannotUseContinueInForLoop()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public class SomeClass
    {
        public void SomeMethod()
        {
            for(int i=0;i<40;i++)
            {
                if (i % 3 == 0)
                    continue;
                Console.WriteLine(i);
            }
        }
    }
}", "");
		}

		[TestMethod]
		public void ConstructorCallsBaseConstructor()
		{
			var cs = @"
using System;

namespace Blargh
{
    public class Top
    {
        public Top(int i) { }
    }

    public class Derived : Top
    {
        public Derived() : base(4) { }
    }
}";

			var haxe1 = @" 
package blargh;
" + WriteImports.StandardImports + @"

class Top
{
    public function new(i:Int)
    {
    }
}";

			var haxe2 = @"
package blargh;
" + WriteImports.StandardImports + @"

class Derived extends blargh.Top
{
    public function new()
    {
        super(4);
    }
}";

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, cs, new string[] { haxe1, haxe2 });
		}

		[TestMethod]
		public void ImportStatements()
		{
			var cSharp = @"
using System;
namespace SomeClassNamespace
{
    using SomeInterfaceNamespace;

    public class SomeClass : ISomeInterface, IDisposable
    {
        public void SomeClassMethod() { }
		public void Dispose() { }
    }
}

namespace SomeInterfaceNamespace
{
    public interface ISomeInterface
    {
    }

    public class UnusedClass
    {
    }
}";

			var haxe1 = @"
package someclassnamespace;
" + WriteImports.StandardImports + @"

class SomeClass implements someinterfacenamespace.ISomeInterface implements system.IDisposable
{
    public function SomeClassMethod():Void
    {
    }

	public function Dispose():Void
	{
	}
    
    public function new()
    {
    }
}";

			var haxe2 = @"
package someinterfacenamespace;
" + WriteImports.StandardImports + @"

interface ISomeInterface
{
}";
			var haxe3 = @"
package someinterfacenamespace;
" + WriteImports.StandardImports + @"

class UnusedClass
{
    public function new()
    {
    }
}";

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, cSharp, new string[] { haxe1, haxe2, haxe3 });
		}

		[TestMethod]
		public void TypeInference()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.Text;

namespace Blargh
{
    public class Box
    {
        public static void Main()
        {
            SomeFunction((_, o) => o + 1);
        }

        public static int SomeFunction(Func<StringBuilder, int, int> doWork)
        {
            var value = doWork(null, 3);
			return value;
        }

        public Box(Action<DateTime> doWork)
        {
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Box
{
    public static function Main():Void
    {
        SomeFunction(function (_:system.text.StringBuilder, o:Int):Int { return o + 1; } );
    }

    public static function SomeFunction(doWork:(system.text.StringBuilder -> Int -> Int)):Int
    {
        var value:Int = doWork(null, 3);
		return value;
    }
    public function new(doWork:(system.DateTime -> Void))
    {
    }
}
");
		}

		[TestMethod]
		[ExpectedException(typeof(AggregateException), "C# 3.5 object initialization syntax is not supported")]
		public void ObjectInitilization()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            var usingMe = new SomeUsingType()
            {
                Init1 = 0
            };
        }
    }
}", "");
		}

		[TestMethod]
		[ExpectedException(typeof(AggregateException), "You cannot return from within a using block")]
		public void CannotReturnFromUsing()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
namespace Blargh
{
    public static class Utilities
    {
        public static int SomeFunction()
        {
            var usingMe = new SomeUsingType();
            using (usingMe)
            {
                return 4;
            }
        }
    }
}", "");
		}

		[TestMethod]
		public void UsingStatement()
		{

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.IO;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            var usingMe = new MemoryStream();
            using (usingMe)
            {
                Console.WriteLine(""In using"");
            }
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        var usingMe:system.io.MemoryStream = new system.io.MemoryStream();
        var __disposed_usingMe:Bool = false;
        try
        {
            system.Console.WriteLine(""In using"");

            __disposed_usingMe = true;
            usingMe.Dispose();
        }
        catch (__catch_usingMe:Dynamic)
        {
            if (!__disposed_usingMe)
                usingMe.Dispose();
            throw __catch_usingMe;
        }
    }
    public function new()
    {
    }
}
");
		}
		[TestMethod]
		public void Math()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            int i = 3;
            i += 4;
            i -= 3;
            ++i;
            i++;
            i--;
            --i;
            i *= 4;
            i %= 3;
            i = i + 1;
            i = i % 3;
            i = i - 4;
            i = i * 100;
            double f = i / 3f;
            int hex = 0x00ff;
            i = (int)f;
			var z = (i & hex) == 5;
			var x = (int)(i / 3);
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        var i:Int = 3;
        i += 4;
        i -= 3;
        ++i;
        i++;
        i--;
        --i;
        i *= 4;
        i %= 3;
        i = i + 1;
        i = i % 3;
        i = i - 4;
        i = i * 100;
        var f:Float = i / 3;
        var hex:Int = 0x00ff;
        i = Std.int(f);
		var z:Bool = (i & hex) == 5;
		var x:Int = Std.int(i / 3);
    }
    public function new()
    {
    }
}");
		}

		[TestMethod]
		public void Delegates()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public delegate int NamespaceDlg();
    public delegate T TemplatedDelegate<T>(T arg, int arg2);

    public static class Utilities
    {
		public static Action StaticAction;
        public delegate int GetMahNumber(int arg);

        public static void SomeFunction(GetMahNumber getit, NamespaceDlg getitnow, TemplatedDelegate<float> unused)
        {
            Console.WriteLine(getit(getitnow()));
            var a = new[] { getitnow };
			a[0]();
			StaticAction();
			Utilities.StaticAction();
			Blargh.Utilities.StaticAction();
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
	public static var StaticAction:(Void -> Void);

    public static function SomeFunction(getit:(Int -> Int), getitnow:(Void -> Int), unused:(Float -> Int -> Float)):Void
    {
        system.Console.WriteLine_Int32(getit(getitnow()));
		var a:Array<(Void -> Int)> = [ getitnow ];
		a[0]();
		StaticAction();
		blargh.Utilities.StaticAction();
		blargh.Utilities.StaticAction();
    }
    public function new()
    {
    }
}");
		}

		[TestMethod]
		public void TypeStatics()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
		static int Foo;
        public static void SomeFunction()
        {
            Blargh.Utilities.Foo = 4;
            Console.WriteLine(int.MaxValue);
            Console.WriteLine(int.MinValue);
            string s = ""123"";
            Console.WriteLine(int.Parse(s) + 1);
            float.Parse(s);
            double.Parse(s);
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
	static var Foo:Int;
    public static function SomeFunction():Void
    {
        blargh.Utilities.Foo = 4;
        system.Console.WriteLine_Int32(2147483647);
        system.Console.WriteLine_Int32(-2147483648);
        var s:String = ""123"";
        system.Console.WriteLine_Int32(Std.parseInt(s) + 1);
        Std.parseFloat(s);
        Std.parseFloat(s);
    }
    public static function cctor():Void
    {
        Foo = 0; 
    }
    public function new()
    {
    }
}");
		}

		[TestMethod]
		public void DictionaryAndHashSet()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.Linq;
using System.Collections.Generic;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            dict.Add(4, 3);
            Console.WriteLine(dict[4]);
            Console.WriteLine(dict.ContainsKey(8));
            dict.Remove(4);
            foreach(int key in dict.Keys)
                Console.WriteLine(key);
            foreach(int val in dict.Values)
                Console.WriteLine(val);
			foreach(var kv in dict)
				Console.WriteLine(kv.Key + "" "" + kv.Value);
			var dict2 = dict.ToDictionary(o => o.Key, o => o.Value);
			var vals = dict.Values;
            
            HashSet<int> hash = new HashSet<int>();
            hash.Add(999);
            Console.WriteLine(hash.Contains(999));
            hash.Remove(999);
            Console.WriteLine(hash.Contains(999));
            foreach(int hashItem in hash)
                Console.WriteLine(hashItem);
			var z = hash.Select(o => 3).ToArray();
			var g = hash.GroupBy(o => o).Select(o => o.Count()).Min();
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        var dict:system.collections.generic.Dictionary<Int, Int> = new system.collections.generic.Dictionary<Int, Int>();
        dict.Add(4, 3);
        system.Console.WriteLine_Int32(dict.GetValue_TKey(4));
        system.Console.WriteLine_Boolean(dict.ContainsKey(8));
        dict.Remove(4);
        for (key in dict.Keys)
        {
            system.Console.WriteLine_Int32(key);
        }
        for (val in dict.Values)
        {
            system.Console.WriteLine_Int32(val);
        }
		for (kv in dict.GetEnumerator())
		{
			system.Console.WriteLine(kv.Key + "" "" + kv.Value);
		}
		var dict2:system.collections.generic.Dictionary<Int, Int> = system.linq.Enumerable.ToDictionary(Cs2Hx.GetEnumeratorNullCheck(dict), function (o:system.collections.generic.KeyValuePair<Int, Int>):Int { return o.Key; } , function (o:system.collections.generic.KeyValuePair<Int, Int>):Int { return o.Value; } );
		var vals:Array<Int> = dict.Values;
        
        var hash:system.collections.generic.HashSet<Int> = new system.collections.generic.HashSet<Int>();
        hash.Add(999);
        system.Console.WriteLine_Boolean(hash.Contains(999));
        hash.Remove(999);
        system.Console.WriteLine_Boolean(hash.Contains(999));
        for (hashItem in hash.GetEnumerator())
        {
            system.Console.WriteLine_Int32(hashItem);
        }
		var z:Array<Int> = system.linq.Enumerable.ToArray(system.linq.Enumerable.Select(Cs2Hx.GetEnumeratorNullCheck(hash), function (o:Int):Int { return 3; } ));
		var g:Int = system.linq.Enumerable.Min(system.linq.Enumerable.Select(system.linq.Enumerable.GroupBy(Cs2Hx.GetEnumeratorNullCheck(hash), function (o:Int):Int { return o; } ), function (o:system.linq.IGrouping<Int, Int>):Int { return system.linq.Enumerable.Count(Cs2Hx.GetEnumeratorNullCheck(o)); } ));

    }
    public function new()
    {
    }
}");
		}

		[TestMethod]
		public void NullableTypes()
		{

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            int? nullableInt = new Nullable<int>();
			double d = 3;
			var cond = nullableInt.HasValue ? (float?)null : ((float)d);
            Console.WriteLine(nullableInt.HasValue);
            int? withValue = new Nullable<int>(8);
            Console.WriteLine(withValue.Value);
			int? implicitNull = null;
			implicitNull = null;
			int? implicitValue = 5;
			implicitValue = 8;
			Foo(3);
			int? n = (int?)null;
        }

		public static int? Foo(int? i)
		{
			return 4;
		}
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        var nullableInt:Nullable_Int = new Nullable_Int();
		var d:Float = 3;
		var cond:Nullable_Float = nullableInt.HasValue ? new Nullable_Float() : (new Nullable_Float(d));
        system.Console.WriteLine_Boolean(nullableInt.HasValue);
        var withValue:Nullable_Int = new Nullable_Int(8);
        system.Console.WriteLine_Int32(withValue.Value);
		var implicitNull:Nullable_Int = new Nullable_Int();
		implicitNull = new Nullable_Int();
		var implicitValue:Nullable_Int = new Nullable_Int(5);
		implicitValue = new Nullable_Int(8);
		Foo(new Nullable_Int(3));
		var n:Nullable_Int = new Nullable_Int();
    }
	public static function Foo(i:Nullable_Int):Nullable_Int
	{
		return new Nullable_Int(4);
	}
    public function new()
    {
    }
}");
		}

		[TestMethod]
		public void NullableDefaults()
		{

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction(int? f = 3, float? s = null)
        {
            Console.WriteLine(""Hello, World!"");
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public static function SomeFunction(f:Nullable_Int = null, s:Nullable_Float = null):Void
    {
		if (f == null)
			f = new Nullable_Int(3);
		if (s == null)
			s = new Nullable_Float();
        system.Console.WriteLine(""Hello, World!"");
    }
    public function new()
    {
    }
}");
		}


		[TestMethod]
		[ExpectedException(typeof(AggregateException), "When using nullable types, you must use the .Value and .HasValue properties instead of the object directly")]
		public void MustUseNullableProperties()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            int? i = 5;
			if (i == null)
			{
			}

        }
    }
}", "");
		}

		[TestMethod]
		public void Enums()
		{
			

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, new string[] { @"
using System;

namespace Blargh
{
    public enum MostlyNumbered
    {
        One = 1,
        Two = 2,
        Three = 3,
        Unnumbered,
        SomethingElse = 50
    }
    public enum UnNumbered
    {
        One, Two, Three
    }
    class Clazz
    {
        public static void Methodz()
        {
            var f = MostlyNumbered.One;
            var arr = new UnNumbered[] { UnNumbered.One, UnNumbered.Two, UnNumbered.Three };
            var i = (int)f;
			var e = (MostlyNumbered)Enum.Parse(typeof(MostlyNumbered), ""One"");
			var s = e.ToString();
			s = e + ""asdf"";
			s = ""asdf"" + e;
			var vals = Enum.GetValues(typeof(MostlyNumbered));
        }
    }
}" }, new string[] { @"
package blargh;
" + WriteImports.StandardImports + @"
class MostlyNumbered
{
    public static inline var One:Int = 1;
    public static inline var Two:Int = 2;
    public static inline var Three:Int = 3;
    public static inline var Unnumbered:Int = 4;
    public static inline var SomethingElse:Int = 50;

	public static function ToString(e:Int):String 
	{ 
		switch (e) 
		{
			case 1: return ""One""; 
			case 2: return ""Two""; 
			case 3: return ""Three""; 
			case 4: return ""Unnumbered""; 
			case 50: return ""SomethingElse""; 
			default: return Std.string(e);
		}
	} 
	
	public static function Parse(s:String):Int 
	{ 
		switch (s) 
		{ 
			case ""One"": return 1; 
			case ""Two"": return 2;		
			case ""Three"": return 3; 
			case ""Unnumbered"": return 4; 
			case ""SomethingElse"": return 50; 
			default: throw new InvalidOperationException(s); 
		} 
	}

	public static function Values():Array<Int>
	{
		return [1, 2, 3, 4, 50];
	}
}", @"
package blargh;
" + WriteImports.StandardImports + @"
class UnNumbered
{
	public static inline var One:Int = 0;
    public static inline var Two:Int = 1;
    public static inline var Three:Int = 2;
	public static function ToString(e:Int):String 
	{ 
		switch (e) 
		{ 
			case 0: return ""One""; 
			case 1: return ""Two""; 
			case 2: return ""Three""; 
			default: return Std.string(e);
		}
	} 
	
	public static function Parse(s:String):Int 
	{ 
		switch (s) 
		{ 
			case ""One"": return 0; 
			case ""Two"": return 1;		
			case ""Three"": return 2; 
			default: throw new InvalidOperationException(s); 
		} 
	}
	public static function Values():Array<Int>
	{
		return [0, 1, 2];
	}
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Clazz
{
    public static function Methodz():Void
    {
        var f:Int = blargh.MostlyNumbered.One;
        var arr:Array<Int> = [ blargh.UnNumbered.One, blargh.UnNumbered.Two, blargh.UnNumbered.Three ];
        var i:Int = f;
		var e:Int = blargh.MostlyNumbered.Parse(""One"");
		var s:String = blargh.MostlyNumbered.ToString(e);
		s = blargh.MostlyNumbered.ToString(e) + ""asdf"";
		s = ""asdf"" + blargh.MostlyNumbered.ToString(e);
		var vals = blargh.MostlyNumbered.Values();
	}
    public function new()
    {
    }
}"});
		}

		[TestMethod]
		public void NestedEnum()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, new string[] { @"
namespace Blargh
{
    class Foo
    {
		public enum TestEnum
		{
			One, Two, Three
		}

		public Foo()
		{
			var i = TestEnum.One;
			i.ToString();
		}
    }
}" }, new string[] { @"
package blargh;
" + WriteImports.StandardImports + @"
class Foo
{
	public function new()
	{
		var i:Int = blargh.Foo_TestEnum.One;
		blargh.Foo_TestEnum.ToString(i);
	}
}", @"
package blargh;
" + WriteImports.StandardImports + @"
class Foo_TestEnum
{
	public static inline var One:Int = 0; 
    public static inline var Two:Int = 1;
    public static inline var Three:Int = 2;

	public static function ToString(e:Int):String 
	{ 
		switch (e) 
		{ 
			case 0: return ""One""; 
			case 1: return ""Two""; 
			case 2: return ""Three""; 
			default: return Std.string(e);
		}
	} 
	
	public static function Parse(s:String):Int 
	{ 
		switch (s) 
		{ 
			case ""One"": return 0; 
			case ""Two"": return 1;		
			case ""Three"": return 2; 
			default: throw new InvalidOperationException(s); 
		} 
	}
	public static function Values():Array<Int>
	{
		return [0, 1, 2];
	}
}" });
		}

		[TestMethod]
		public void SwitchStatement()
		{

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            string s = ""Blah"";
            switch (s)
            {
                case ""NotMe"": Console.WriteLine(5); break;
                case ""Box"": Console.WriteLine(4); break;
                case ""Blah"": 
				case ""Blah2"": Console.WriteLine(3); break;
                default: throw new InvalidOperationException();
            }
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        var s:String = ""Blah"";
        switch (s)
        {
            case ""NotMe"":
                system.Console.WriteLine_Int32(5);
            case ""Box"": 
                system.Console.WriteLine_Int32(4); 
            case ""Blah"", ""Blah2"": 
                system.Console.WriteLine_Int32(3); 
            default: 
                throw new system.InvalidOperationException();
        }
    }
    public function new()
    {
    }
}");
		}
		[TestMethod]
		public void Linq()
		{

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.Linq;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            int[] e = new int[] { 0, 1, 2, 3 };
            Console.WriteLine(e.First());
            Console.WriteLine(e.First(o => o == 1));
            Console.WriteLine(e.ElementAt(2));
            Console.WriteLine(e.Last());
            Console.WriteLine(e.Select(o => o).Count());
            Console.WriteLine(e.Where(o => o > 0).Count() + 2);
            Console.WriteLine(e.Count(o => true) + 2);

            var dict = e.ToDictionary(o => o, o => 555);
            e.OfType<int>();
			e.OrderBy(o => 4);
			e.OrderBy(o => ""z"");
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        var e:Array<Int> = [ 0, 1, 2, 3 ];
        system.Console.WriteLine_Int32(system.linq.Enumerable.First(e));
		
        system.Console.WriteLine_Int32(system.linq.Enumerable.First_IEnumerable_FuncBoolean(e, function (o:Int):Bool
        {
            return o == 1;
        } ));
        system.Console.WriteLine_Int32(system.linq.Enumerable.ElementAt(e, 2));
        system.Console.WriteLine_Int32(system.linq.Enumerable.Last(e));
        system.Console.WriteLine_Int32(system.linq.Enumerable.Count(system.linq.Enumerable.Select(e, function (o:Int):Int { return o; } )));
        system.Console.WriteLine_Int32(system.linq.Enumerable.Count(system.linq.Enumerable.Where(e, function (o:Int):Bool
        {
            return o > 0;
        } )) + 2);
        system.Console.WriteLine_Int32(system.linq.Enumerable.Count_IEnumerable_FuncBoolean(e, function (o:Int):Bool
        {
            return true;
        } ) + 2);
        var dict:system.collections.generic.Dictionary<Int, Int> = system.linq.Enumerable.ToDictionary(e, function (o:Int):Int
        {
            return o;
        } , function (o:Int):Int
        {
            return 555;
        } );
        system.linq.Enumerable.OfType(e, Int);
		system.linq.Enumerable.OrderBy_Int(e, function (o:Int):Int { return 4; } );
		system.linq.Enumerable.OrderBy_String(e, function (o:Int):String { return ""z""; } );
    }
    public function new()
    {
    }
}");
		}

		[TestMethod]
		public void OverloadedMethods()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
        public static void OverOne()
        {
            OverOne(3);
			Math.Max(3, 3);
			Math.Max(4.0, 4.0);
			Math.Max(5f, 5f);
        }

        public static void OverOne(int param)
        {
            OverOne(param, ""Blah"");
        }

        public static void OverOne(int param, string prm)
        {
            Console.WriteLine(param + prm);
        }

        public static int OverTwo()
        {
            return OverTwo(18);
        }
        public static int OverTwo(int prm)
        {
            return prm;
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
	public static function OverOne():Void
	{
		OverOne_Int32(3);
		system.MathCS.Max_Int32_Int32(3, 3);
		system.MathCS.Max_Double_Double(4.0, 4.0);
		system.MathCS.Max_Single_Single(5, 5);
	}
	public static function OverOne_Int32(param:Int):Void
	{
		OverOne_Int32_String(param, ""Blah"");
	}
    public static function OverOne_Int32_String(param:Int, prm:String):Void
    {
        system.Console.WriteLine(param + prm);
    }
	public static function OverTwo():Int
	{
		return OverTwo_Int32(18);
	}
    public static function OverTwo_Int32(prm:Int):Int
    {
        return prm;
    }
    public function new()
    {
    }
}");
		}


		[TestMethod]
		public void IsAndAs()
		{

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.Collections.Generic;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            string s = ""Blah"";
            var list = new List<int>();
            if (s is string)
                Console.WriteLine(""Yes"");
            if (list is List<int>)
                Console.WriteLine(""Yes"");

//            object o = s;
//            string sss = o as string;
//            Console.WriteLine(sss);
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        var s:String = ""Blah"";
        var list:Array<Int> = new Array<Int>();
        if (Std.is(s, String))
        {
            system.Console.WriteLine(""Yes"");
        }
        if (Std.is(list, Array))
        {
            system.Console.WriteLine(""Yes"");
        }
    }
    public function new()
    {
    }
}");
		}


		[TestMethod]
		public void AbstractAndOverrides()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    abstract class TopLevel
    {
        public abstract void AbstractMethod();
        public abstract string AbstractProperty { get; }

        public virtual void VirtualMethod()
        {
            Console.WriteLine(""TopLevel::VirtualMethod"");
        }
        public virtual string VirtualProperty
        {
            get
            {
                return ""TopLevel::VirtualProperty"";
            }
        }

        public override string ToString()
        {
            return string.Empty;
        }
    }

    class Derived : TopLevel
    {
        public override void AbstractMethod()
        {
            Console.WriteLine(""Derived::AbstractMethod"");
        }

        public override string AbstractProperty
        {
            get { return ""Derived::AbstractProperty""; }
        }

        public override void VirtualMethod()
        {
            base.VirtualMethod();
            Console.WriteLine(""Derived::VirtualMethod"");
        }

        public override string VirtualProperty
        {
            get
            {
                return base.VirtualProperty + ""Derived:VirtualProperty"";
            }
        }
        public override string ToString()
        {
            return ""DerivedToString"";
        }
    }
}", new string[] {
      @"
package blargh;
" + WriteImports.StandardImports + @"

class TopLevel
{
    public function AbstractMethod():Void
    {
    	throw new Exception(""Abstract item called"");
    }
	public var AbstractProperty(get_AbstractProperty, never):String;
	
    public function get_AbstractProperty():String
    {
    	return throw new Exception(""Abstract item called"");
    }

    public function VirtualMethod():Void
    {
        system.Console.WriteLine(""TopLevel::VirtualMethod"");
    }

    public var VirtualProperty(get_VirtualProperty, never):String;
    public function get_VirtualProperty():String
    {
        return ""TopLevel::VirtualProperty"";
    }

    public function toString():String
    {
        return """";
    }
	
	public function new()
	{
	}
}",
    @"
package blargh;
" + WriteImports.StandardImports + @"

class Derived extends blargh.TopLevel
{
    override public function AbstractMethod():Void
    {
        system.Console.WriteLine(""Derived::AbstractMethod"");
    }
    override public function get_AbstractProperty():String
    {
        return ""Derived::AbstractProperty"";
    }
    override public function VirtualMethod():Void
    {
        super.VirtualMethod();
        system.Console.WriteLine(""Derived::VirtualMethod"");
    }

    override public function get_VirtualProperty():String
    {
        return super.VirtualProperty + ""Derived:VirtualProperty"";
    }

    override public function toString():String
    {
        return ""DerivedToString"";
    }
	
	public function new()
	{
		super();
	}
}"  });
		}


		[TestMethod]
		public void FieldsAndProperties()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.Text;

namespace Blargh
{
    class Box
    {
        private float _width;
        public float Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public float SetOnly
        {
            set { Console.WriteLine(value); }
        }

        public int GetOnly
        {
            get { return 4; }
        }
        
        public bool IsRectangular = true;
        public char[] Characters = new char[] { 'a', 'b' };
        public static StringBuilder StaticField = new StringBuilder();
        public const int ConstInt = 24;
        public static readonly int StaticReadonlyInt = 5;
        public const string WithQuoteMiddle = @""before""""after"";
        public const string WithQuoteStart = @""""""after"";
        public int MultipleOne, MultipleTwo;
        public readonly int ReadonlyInt = 3;
		public DateTime UninitializedDate;
		public int? UnitializedNullableInt;
		public TimeSpan UninitializedTimeSpan;
		public static DateTime StaticUninitializedDate;
		public static int? StaticUnitializedNullableInt;
		public static TimeSpan StaticUninitializedTimeSpan;

        static Box()
        {
            Console.WriteLine(""cctor"");
        }

        public Box()
        {
            Console.WriteLine(""ctor"");
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Box
{
    private var _width:Float;
    public var Width(get_Width, set_Width):Float;
    public function get_Width():Float
    {
        return _width;
    }
    public function set_Width(value:Float):Float
    {
        _width = value;
		return value;
    }
    public var SetOnly(never, set_SetOnly):Float;
    public function set_SetOnly(value:Float):Float
    {
        system.Console.WriteLine_Single(value);
		return value;
    }
    public var GetOnly(get_GetOnly, never):Int;
    public function get_GetOnly():Int
    {
        return 4;
    }

    public var IsRectangular:Bool;
    public var Characters:Array<Int>;
    public static var StaticField:system.text.StringBuilder;
    public static inline var ConstInt:Int = 24;
    public static inline var StaticReadonlyInt:Int = 5;
    public static inline var WithQuoteMiddle:String = ""before\""after"";
    public static inline var WithQuoteStart:String = ""\""after"";
    public var MultipleOne:Int;
    public var MultipleTwo:Int;
    public var ReadonlyInt:Int;
	public var UninitializedDate:system.DateTime;
	public var UnitializedNullableInt:Nullable_Int;
	public var UninitializedTimeSpan:system.TimeSpan;
	public static var StaticUninitializedDate:system.DateTime;
	public static var StaticUnitializedNullableInt:Nullable_Int;
	public static var StaticUninitializedTimeSpan:system.TimeSpan;


    public static function cctor():Void
    {
        StaticField = new system.text.StringBuilder();
		StaticUninitializedDate = new system.DateTime();
		StaticUnitializedNullableInt = new Nullable_Int();
		StaticUninitializedTimeSpan = new system.TimeSpan();
        system.Console.WriteLine(""cctor"");
    }

	public function new()
	{
        _width = 0;
		IsRectangular = true;
		Characters = [ 97, 98 ];
        MultipleOne = 0;
        MultipleTwo = 0;
        ReadonlyInt = 3;
		UninitializedDate = new system.DateTime();
		UnitializedNullableInt = new Nullable_Int();
		UninitializedTimeSpan = new system.TimeSpan();
        system.Console.WriteLine(""ctor"");
	}
}");
		}



		[TestMethod]
		public void Interfaces()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public interface ITesting
    {
        void Poke();
    }

    class Pokable : ITesting
    {
        public void Poke()
        {
            Console.WriteLine(""Implementation"");
        }
    }
}",
  new string[] { @"
package blargh;
" + WriteImports.StandardImports + @"

interface ITesting
{
    function Poke():Void;
}",
  @"
package blargh;
" + WriteImports.StandardImports + @"

class Pokable implements blargh.ITesting
{
    public function Poke():Void
    {
        system.Console.WriteLine(""Implementation"");
    }
    public function new()
    {
    }
}"});
		}

		[TestMethod]
		public void TryCatchThrow()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.IO;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            Console.WriteLine(""Before try"");
            try
            {
                Console.WriteLine(""In try"");
            }
            catch (Exception ex)
            {
                Console.WriteLine(""In catch "" + ex + "" "" + ex.ToString());
                TakesObject(ex);
            }

            try
            {
                Console.WriteLine(""Try without finally"");
            }
            catch (IOException ex)
            {
                Console.WriteLine(""In second catch"");
            }

            try
            {
                Console.WriteLine(""Try in parameterless catch"");
            }
            catch
            {
                Console.WriteLine(""In parameterless catch"");
            }

            throw new InvalidOperationException(""err"");
        }

        public static void TakesObject(object o) { }

		public static string ReturnsSomething()
		{
			throw new Exception();
		}
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        system.Console.WriteLine(""Before try"");
        try
        {
            system.Console.WriteLine(""In try"");
        }
        catch (ex:Dynamic)
        {
            system.Console.WriteLine(""In catch "" + ex + "" "" + ex.toString());
            TakesObject(ex);
        }
        try
        {
            system.Console.WriteLine(""Try without finally"");
        }
        catch (ex:system.io.IOException)
        {
            system.Console.WriteLine(""In second catch"");
        }
        try
        {
            system.Console.WriteLine(""Try in parameterless catch"");
        }
        catch (__ex:Dynamic)
        {
            system.Console.WriteLine(""In parameterless catch"");
        }

        throw new system.InvalidOperationException(""err"");
    }
    public static function TakesObject(o:Dynamic):Void
    {
    }
	public static function ReturnsSomething():String
	{
		return throw new system.Exception();
	}
    public function new()
    {
    }
}");
		}


        [ExpectedException(typeof(AggregateException), "When catching an Exception, you cannot use the object as an Exception object.")]
        [TestMethod]
        public void CatchException()
        {
            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.IO;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw new Exception("""", ex);
            }
        }

    }
}", @"");
        }


        [ExpectedException(typeof(AggregateException), "When catching an Exception, you cannot use the object as an Exception object.")]
        [TestMethod]
        public void CatchException2()
        {
            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.IO;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            try
            {
            }
            catch (Exception ex)
            {
                TakesEx(ex);
            }
        }

        public static void TakesEx(Exception ex)
        {
        }

    }
}", @"");
        }


        [TestMethod]
		public void Generics()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.Collections.Generic;

namespace Blargh
{
    public static class Utilities
    {
        public static Queue<T> ToQueue<T>(this IEnumerable<T> array)
        {
            var queue = new Queue<T>();
            foreach (T a in array)
                queue.Enqueue(a);

            queue.Dequeue();
            return queue;
        }

        public static IEnumerable<T> SideEffect<T>(this IEnumerable<T> array, Action<T> effect)
        {
            foreach(var i in array)
                effect(i);
            return array;
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public static function ToQueue<T>(array:Array<T>):Array<T>
    {
        var queue:Array<T> = new Array<T>();
        for (a in array)
        {
            queue.push(a);
        }
        queue.shift();
        return queue;
    }
    public static function SideEffect<T>(array:Array<T>, effect:(T -> Void)):Array<T>
    {
        for (i in array)
        {
            effect(i);
        }
        return array;
    }
    public function new()
    {
    }
}");
		}


        [TestMethod]
        public void GenericNoParam()
        {
            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.Linq;
using System.Collections.Generic;

namespace Blargh
{
    public class Foo
    {
        public static void ToQueue<T>()
        {
        }

        public Foo()
        {
            ToQueue<int>();
            ToQueue<Foo>();
            var l = ToList(1,2,3);
        }


        public static List<T> ToList<T>(params T[] items)
        {
            return items.ToList();
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Foo
{
    public static function ToQueue<T>(t1:Class<T>):Void
    {
    }
    public function new()
    {
        ToQueue(Int);
        ToQueue(blargh.Foo);
        var l:Array<Int> = ToList([ 1, 2, 3 ]);
    }

    public static function ToList<T>(items:Array<T>):Array<T>
    {
        return system.linq.Enumerable.ToList(items);
    }
}");
        }

        [TestMethod]
		public void Objects()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.Collections.Generic;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            var queue = new Queue<int>(10);
            queue.Enqueue(4);
            queue.Enqueue(2);
            Console.WriteLine(queue.Dequeue());
            queue.Clear();
    
            var list = new List<string>(3);
            list.Add(""Three"");
            list.RemoveAt(0);
            list.Insert(4, ""Seven"");
			list.Sort();
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        var queue:Array<Int> = new Array<Int>();
        queue.push(4);
        queue.push(2);
        system.Console.WriteLine_Int32(queue.shift());
        system.Cs2Hx.Clear(queue);

        var list:Array<String> = new Array<String>();
        list.push(""Three"");
        list.splice(0, 1);
        list.insert(4, ""Seven"");
		list.sort(Cs2Hx.SortStrings);
    }
    public function new()
    {
    }
}");
		}

		[TestMethod]
		public void Lambda()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.Collections.Generic;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            Func<int, int> f1 = x => x + 5;
            Console.WriteLine(f1(3));
            Func<int, int> f2 = x => { return x + 6; };
            Console.WriteLine(f2(3));

            List<Action> actions = new List<Action>();
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        var f1:(Int -> Int) = function (x:Int):Int 
        { 
            return x + 5; 
        } ;
        system.Console.WriteLine_Int32(f1(3));
        var f2:(Int -> Int) = function (x:Int):Int 
        { 
            return x + 6; 
        } ;
        system.Console.WriteLine_Int32(f2(3));
        var actions:Array<(Void -> Void)> = new Array<(Void -> Void)>();
    }
    public function new()
    {
    }
}");
		}

		[TestMethod]
		public void LambdaNoReturn()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            int i = 3;
            Action a = () => i = 4;
            Func<int> b = () => i = 5;
            Foo(() => i = 6);
        }
        public static void Foo(Action a)
        {
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        var i:Int = 3;
        var a:(Void -> Void) = function ():Void
        { 
            i = 4;
        } ;
        var b:(Void -> Int) = function ():Int
        { 
            return i = 5;
        } ;
        Foo(function ():Void
        {
            i = 6;
        } );
    }
    public static function Foo(a:(Void -> Void)):Void
    {
    }
    public function new()
    {
    }
}");
		}
		

		[TestMethod]
		public void Casts()
		{
			var cs = @"
using System;

namespace Blargh
{
#if !CS2HX
	public static class Utilities
	{
		public static T As<T>(this object o)
		{
			return (T)o;
		}
	}
#endif

    public static class Test
    {
        public static void SomeFunction()
        {
			var a = DateTime.Now.As<String>();
			object o = 4;
			var b = (byte)(short)o;
        }
    }
}";
  
			var haxe = @"
package blargh;
" + WriteImports.StandardImports + @"

class Test
{
    public static function SomeFunction():Void
    {
        var a:String = cast(system.DateTime.Now, String);
		var o:Dynamic = 4;
		var b:Int = o;
    }
    public function new()
    {
    }
}";

			var transform = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Translations>
  <Method SourceObject=""*"" Match=""As"">
    <ReplaceWith>
      <String>cast(</String>
      <Expression />
      <String>, {genericType})</String>
    </ReplaceWith>
  </Method>
</Translations>";

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, new[] { cs }, new[] { haxe }, transform);
		}


		[TestMethod]
		public void ArrayAndForEach()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.Collections.Generic;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            var ar = new int[] { 1, 2, 3 };

            foreach(var i in ar)
                Console.WriteLine(i);

            Console.WriteLine(ar[1]);
            Console.WriteLine(ar.Length);
			Console.WriteLine(new List<string>().Count);
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        var ar:Array<Int> = [ 1, 2, 3 ];
        for (i in ar)
        {
        	system.Console.WriteLine_Int32(i);
        }
        system.Console.WriteLine_Int32(ar[1]);
        system.Console.WriteLine_Int32(ar.length);
		system.Console.WriteLine_Int32(new Array<String>().length);
    }
    public function new()
    {
    }
}");
		}

		[TestMethod]
		public void PartialClasses()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name,

new string[] { @"
using System;

namespace Blargh
{
    public partial class Utilities
    {
        public void FunFromOne()
        {
            Console.WriteLine(""I'm in one!"");
        }
    }
}",
            
  @"
using System;

namespace Blargh
{
    public partial class Utilities
    {
        public void FunFromTwo()
        {
            Console.WriteLine(""I'm in Two!"");
        }
    }
}"
}, @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public function FunFromOne():Void
    {
        system.Console.WriteLine(""I'm in one!"");
    }
    public function FunFromTwo():Void
    {
        system.Console.WriteLine(""I'm in Two!"");
    }
    public function new()
    {
    }
}");
		}

		[TestMethod]
		public void StringMethods()
		{

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction(string s2)
        {
            string s = @""50\0"";
            Console.WriteLine(s.IndexOf(""0""));
            Console.WriteLine(s2.IndexOf(""0""));

            foreach(string s3 in new string[] { ""Hello"" })
                s3.Substring(4, 5);

            int i = 4;
            string si = i.ToString();
			if (si.StartsWith(""asdf""))
				Console.WriteLine(4);
            char c = 'A';
            s = c.ToString();
            s = c + """";
            s = """" + c;
            var c2 = c + 3;

            var rep = new String(c, 34);
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"
class Utilities
{
    public static function SomeFunction(s2:String):Void
    {
        var s:String = ""50\\0"";
        system.Console.WriteLine_Int32(s.indexOf(""0""));
        system.Console.WriteLine_Int32(s2.indexOf(""0""));

        for (s3 in [ ""Hello"" ])
        {
            s3.substr(4, 5);
        }
        var i:Int = 4;
        var si:String = Std.string(i);
		if (system.Cs2Hx.StartsWith(si, ""asdf""))
		{
			system.Console.WriteLine_Int32(4);
		}
        var c:Int = 65;
        s = Cs2Hx.CharToString(c);
        s = Cs2Hx.CharToString(c) + """";
        s = """" + Cs2Hx.CharToString(c);
        var c2:Int = c + 3;
        var rep:String = Cs2Hx.NewString(c, 34);
    }
    public function new()
    {
    }
}");
		}

		[TestMethod]
		public void ExtensionMethod()
		{

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            int i = -3;
            Console.WriteLine(""false "" + i.IsFour());
            i++;
            i += 6;
            var b = i.IsFour();
            Console.WriteLine(""true "" + b);
            Utilities.IsFour(5);
        }

        public static bool IsFour(this int i)
        {
            return i == 4;
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        var i:Int = -3;
        system.Console.WriteLine(""false "" + blargh.Utilities.IsFour(i));
        i++;
        i += 6;
        var b:Bool = blargh.Utilities.IsFour(i);
        system.Console.WriteLine(""true "" + b);
        blargh.Utilities.IsFour(5);
    }

    public static function IsFour(i:Int):Bool
    {
        return i == 4;
    }
    public function new()
    {
    }
}");
		}

		[TestMethod]
		public void StringJoin()
		{

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public class Foo
    {
        public Foo()
        {
            var s = string.Join("";"", new[] { ""one"", ""two"" });
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Foo
{
    public function new()
    {
        var s:String = system.Cs2Hx.Join("";"", [ ""one"", ""two"" ]);
    }
}");
		}


		[TestMethod]
		public void RefAndOut()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.Text;
		
namespace Blargh
{
	public class Foo
	{
		public Foo()
		{
			int x;
			TestOut(out x);
			x = 3;
			var s = x.ToString();
			int i = 1;
			TestRef(ref i);
			i = 5;
			new StringBuilder(i);
			Func<int> fun = () => x;
		}
		
		public void TestRef(ref int i)
		{
			var sb = new StringBuilder(i);
			i = 4;
		}
		public void TestOut(out int i)
		{
			i = 4;
			var sb = new StringBuilder(i);
		}
		
	}
}", @"
package blargh;
" + WriteImports.StandardImports + @"
		
class Foo
{
	public function new()
	{
		var x:CsRef<Int> = new CsRef<Int>(0);
		TestOut(x);
		x.Value = 3;
		var s:String = Std.string(x.Value);
		var i:CsRef<Int> = new CsRef<Int>(1);
		TestRef(i);
		i.Value = 5;
		new system.text.StringBuilder(i.Value);
		var fun:(Void -> Int) = function ():Int { return x.Value; } ;
	}
		
	public function TestRef(i:CsRef<Int>):Void
	{
		var sb:system.text.StringBuilder = new system.text.StringBuilder(i.Value);
		i.Value = 4;
	}
		
	public function TestOut(i:CsRef<Int>):Void
	{
		i.Value = 4;
		var sb:system.text.StringBuilder = new system.text.StringBuilder(i.Value);
	}
		
}");
		}

		[TestMethod]
		public void MemoryStream()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.IO;

namespace Blargh
{
    public class Foo
    {
        public Foo()
        {
			var s = new MemoryStream(5);
			var b = new byte[4];
			s = new MemoryStream(b);
			s = new MemoryStream(b.Length);
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Foo
{
    public function new()
    {
		var s:system.io.MemoryStream = new system.io.MemoryStream();
		var b:haxe.io.Bytes = haxe.io.Bytes.alloc(4);
		s = new system.io.MemoryStream(b);
		s = new system.io.MemoryStream();
    }
}");
		}


		[TestMethod]
		public void PartialMethods()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public partial class Foo
    {
        partial void NoOther();
		partial void Other();
    }

	partial class Foo
	{
		partial void Other()
		{
			Console.WriteLine();
		}
	}
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Foo
{
	function NoOther():Void
	{
	}
	function Other():Void
	{
		system.Console.WriteLine();
	}
    public function new()
    {
    }
}");
		}


		[TestMethod]
		public void TypeConstraints()
		{
			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction<T>() where T : class, IComparable<T>
        {
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Utilities
{
    public static function SomeFunction<T: (system.IComparable<T>)>(t1:Class<T>):Void
    {
    }
    public function new()
    {
    }
}");
		}

		[TestMethod]
		public void ExplicitCastOperators()
		{

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Foo
{
    public class Bar
    {
        public static explicit operator string(Bar value)
		{
			return ""blah"";
		}

		public static void Foo()
		{
			var b = new Bar();
			var s = (string)b;
	
		}
    }
}", @"
package foo;
" + WriteImports.StandardImports + @"

class Bar
{
    public static function op_Explicit_String(value:foo.Bar):String
    {
        return ""blah"";
    }
	public static function Foo():Void
	{
		var b:foo.Bar = new foo.Bar();
		var s:String = foo.Bar.op_Explicit_String(b);
	}
    public function new()
    {
    }
}");
		}



		[TestMethod]
		public void ParamsArguments()
		{

			TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Foo
{
    public class Bar
    {
        public static void Method1(params int[] p)
		{
		}
        public static void Method2(int i, params int[] p)
		{
		}
        public static void Method3(int i, int z, params int[] p)
		{
		}

		public static void Foo()
		{
			Method1(1);
			Method1(1, 2);
			Method1(1, 2, 3);
			Method1(1, 2, 3, 4);
			Method2(1);
			Method2(1, 2);
			Method2(1, 2, 3);
			Method2(1, 2, 3, 4);
			Method3(1, 2);
			Method3(1, 2, 3);
			Method3(1, 2, 3, 4);

		}
    }
}", @"
package foo;
" + WriteImports.StandardImports + @"

class Bar
{
    public static function Method1(p:Array<Int>):Void
	{
	}
    public static function Method2(i:Int, p:Array<Int>):Void
	{
	}
    public static function Method3(i:Int, z:Int, p:Array<Int>):Void
	{
	}
	public static function Foo():Void
	{
		Method1([ 1 ]);
		Method1([ 1, 2 ]);
		Method1([ 1, 2, 3 ]);
		Method1([ 1, 2, 3, 4 ]);
		Method2(1);
		Method2(1, [ 2 ]);
		Method2(1, [ 2, 3 ]);
		Method2(1, [ 2, 3, 4 ]);
		Method3(1, 2);
		Method3(1, 2, [ 3 ]);
		Method3(1, 2, [ 3, 4 ]);
	}
    public function new()
    {
    }
}");
		}


        [TestMethod]
        public void PassNullToEnumerableExtensionMethod()
        {

            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.Collections.Generic;

namespace NS
{
    public static class C
    {
        public static void Foo<T>(this IEnumerable<T> a)
		{
		}
		public static void Bar()
		{
			Dictionary<int, string> dict = null;
            dict.Foo();
		}
    }
}", @"
package ns;
" + WriteImports.StandardImports + @"

class C
{
    public static function Foo<T>(a:Array<T>):Void
	{
	}
    public static function Bar():Void
	{
        var dict:system.collections.generic.Dictionary<Int, String> = null;
        ns.C.Foo(Cs2Hx.GetEnumeratorNullCheck(dict));
	}
    public function new() { }
}");
        }


        [TestMethod]
        public void OverloadedOperators()
        {

            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{

    class Vector
    {
        public float X;
        public float Y;

        public Vector(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.X + b.X, a.Y + b.Y);
        }


        public static void SomeFunction()
        {
            var v1 = new Vector(1, 1);
            var v2 = new Vector(1, 1);
            var v3 = v1 + v2;
            v3 += v1;
        }
    }

}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Vector
{
    public var X:Float;
    public var Y:Float;
    public function new(x:Float, y:Float)
    {
        X = 0;
        Y = 0;
        this.X = x;
        this.Y = y;
    }

    public static function op_Addition(a:blargh.Vector, b:blargh.Vector):blargh.Vector
    {
        return new blargh.Vector(a.X + b.X, a.Y + b.Y);
    }

    public static function SomeFunction():Void
    {
        var v1:blargh.Vector = new blargh.Vector(1, 1);
        var v2:blargh.Vector = new blargh.Vector(1, 1);
        var v3:blargh.Vector = blargh.Vector.op_Addition(v1, v2);
        v3 = blargh.Vector.op_Addition(v3, v1);
    }
}");
        }


        [TestMethod]
        public void IndexerDeclarations()
        {

            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{

    class Vector
    {
        public float X;
        public float Y;

        public Vector(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }


        public float this[int index]
        {
            get { return index != 0 ? Y : X; }
            set
            {
                if (index != 0)
                    this.Y = value;
                else
                    this.X = value;
            }
        }

        public static void SomeFunction()
        {
            var v1 = new Vector(1, 1);
            var x = v1[0];
            v1[1] = 3;
        }
    }

}", @"
package blargh;
" + WriteImports.StandardImports + @"

class Vector
{
    public var X:Float;
    public var Y:Float;
    public function new(x:Float, y:Float)
    {
        X = 0;
        Y = 0;
        this.X = x;
        this.Y = y;
    }

    public function GetValue_Int32(index:Int):Float
    {
        return index != 0 ? Y : X;
    }

    public function SetValue_Int32(index:Int, value:Float):Void
    {
        if (index != 0)
        {
            this.Y = value;
        }
        else
        {
            this.X = value;
        }
    }

    public static function SomeFunction():Void
    {
        var v1:blargh.Vector = new blargh.Vector(1, 1);
        var x:Float = v1.GetValue_Int32(0);
        v1.SetValue_Int32(1, 3);
    }
}");
        }


        [TestMethod]
        public void GetEnumeratorMethods()
        {
            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;
using System.Collections;
using System.Collections.Generic;

namespace Blargh
{
    public class SomeCollection : IEnumerable<string>
    {
        private List<string> coll = new List<string>();
            
        public IEnumerator<string> GetEnumerator()
        {
            return coll.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return coll.GetEnumerator();
        }
    }
}", @"
package blargh;
" + WriteImports.StandardImports + @"

class SomeCollection
{
    private var coll:Array<String>;

    public function iterator():Iterator<String>
    {
        return coll.iterator();
    }
	public function new()
	{
        coll = new Array<String>();
	}


}");
        }
    }
}
