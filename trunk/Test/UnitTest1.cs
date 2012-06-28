using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using NUnit.Framework;
using Cs2hx;

namespace Test
{
    [TestFixture]
    class UnitTest1
    {
        [Test]
        public void GlobalKeyword()
        {

            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class SomeClass
    {
        public SomeClass()
        {
            global::Blargh.SomeClass c = null;
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"

class SomeClass
{
    public function new()
    {
        var c:Blargh.SomeClass = null;
    }
}");
        }

        [Test]
        public void DefaultParameter()
        {

            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class SomeClass
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
" + Program.StandardImports + @"

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

        [Test]
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
                trace(""Hello, World!"");
            }
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        { //for
            while (true)
            {
                trace(""Hello, World!"");
            }
        } //end for
    }
    public function new()
    {
    }
}");
        }

        [Test]
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
" + Program.StandardImports + @"

class Box
{
    public var Width:Float;
    public function new()
    {
    }
}");
        }

        [Test]
        public void GenericClass()
        {
            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public class KeyValueList<K,V> : ISomeInterface<K>
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
            var castTest = (K)Foo();
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"
import system.collections.generic.KeyValuePair;

class KeyValueList<K, V> implements ISomeInterface<K>
{
    private var _list:Array<KeyValuePair<K, V>>;

    public function Add(key:K, value:V):Void
    {
        this._list.push(new KeyValuePair<K, V>(key, value));
    }
    public function Insert(index:Int, key:K, value:V):Void
    {
        _list.insert(index, new KeyValuePair<K, V>(key, value));
    }
    public function Clear():Void
    {
        _list.splice(0, _list.length);
        var castTest:K = Foo();
    }
    public function RemoveAt(index:Int):Void
    {
        _list.splice(index, 1);
    }
    public function new()
    {
        _list = new Array<KeyValuePair<K, V>>();
    }
}");
        }

		[Test]
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
			byte[] b2 = new byte[SomeFunction()];
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"
import haxe.io.Bytes;

class Utilities
{
    public static function SomeFunction():Void
    {
        var b1:Bytes = Bytes.alloc(4);
		var b2:Bytes = Bytes.alloc(SomeFunction());
    }
    public function new()
    {
    }
}");
		}

        [Test]
        [ExpectedException(ExpectedMessage = "Cannot use \"continue\" in a \"for\" loop", MatchType = MessageMatch.StartsWith)]
        public void CannotUseContinueInForLoop()
        {
            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class SomeClass
    {
        public void SomeMethod()
        {
            for(int i=0;i<40;i++)
            {
                if (i % 3 == 0)
                    continue;
                trace(i);
            }
        }
    }
}", ""); 
        }

        [Test]
        public void ConstructorCallsBaseConstructor()
        {
            var cs = @"
using System;

namespace Blargh
{
    public static class Top
    {
        public Top(int i) { }
    }

    public static class Derived : Top
    {
        public Derived() : base(4) { }
    }
}";

            var haxe1 = @" 
package blargh;
" + Program.StandardImports + @"

class Top
{
    public function new(i:Int)
    {
    }
}";

            var haxe2 = @"
package blargh;
" + Program.StandardImports + @"

class Derived extends Top
{
    public function new()
    {
        super(4);
    }
}";

            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, cs, new string[] { haxe1, haxe2 }); 
        }

        [Test]
        public void ImportStatements()
        {
            var cSharp = @"
namespace SomeClassNamespace
{
    using SomeInterfaceNamespace;

    public class SomeClass : ISomeInterface
    {
        public void SomeClassMethod() { }
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
" + Program.StandardImports + @"
import someinterfacenamespace.ISomeInterface;

class SomeClass implements ISomeInterface
{
    public function SomeClassMethod():Void
    {
    }
    
    public function new()
    {
    }
}";

            var haxe2 = @"
package someinterfacenamespace;
" + Program.StandardImports + @"

interface ISomeInterface
{
}";
            var haxe3 = @"
package someinterfacenamespace;
" + Program.StandardImports + @"

class UnusedClass
{
    public function new()
    {
    }
}";

            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, cSharp, new string[] { haxe1, haxe2, haxe3 });
        }

        [Test]
        public void TypeInference()
        {
            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
namespace Blargh
{
    public class Box
    {
        public static void Main()
        {
            SomeFunction(o => o + 1);
        }

        public static int SomeFunction(Func<StringBuilder, int> doWork)
        {
            var value = SomeOtherClass.SomeOtherMethod(doWork(3));
        }

        public Box(Action<DateTime> doWork)
        {
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"
import system.DateTime;
import system.text.StringBuilder;

class Box
{
    public static function Main():Void
    {
        SomeFunction(function (o) { return o + 1; } );
    }

    public static function SomeFunction(doWork:(StringBuilder -> Int)):Int
    {
        var value = SomeOtherClass.SomeOtherMethod(doWork(3));
    }
    public function new(doWork:(DateTime -> Void))
    {
    }
}
");
        }

        [Test]
        [ExpectedException(ExpectedMessage = "C# 3.5 object initialization syntax is not supported", MatchType = MessageMatch.StartsWith)]
        public void ObjectInitilization()
        {
            TestFramework.TestCode("test", @"
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

        [Test]
        [ExpectedException(ExpectedMessage = "You cannot return from within a using block", MatchType = MessageMatch.StartsWith)]
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

        [Test]
        public void UsingStatement()
        {

            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            var usingMe = new SomeUsingType();
            using (usingMe)
            {
                trace(""In using"");
            }
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        var usingMe:SomeUsingType = new SomeUsingType();
        var __disposed_usingMe:Bool = false;
        try
        {
            trace(""In using"");

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
        [Test]
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
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"

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
    }
    public function new()
    {
    }
}");
        }

        [Test]
        public void Delegates()
        {
            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
namespace Blargh
{
    public delegate int NamespaceDlg();
    public delegate T TemplatedDelegate<T>(T arg, Int arg2);

    public static class Utilities
    {
        public delegate int GetMahNumber(int arg);

        public static void SomeFunction(GetMahNumber getit, NamespaceDlg getitnow, TemplatedDelegate<Float> unused)
        {
            trace(getit(getitnow()));
            
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"

class Utilities
{
    public static function SomeFunction(getit:(Int -> Int), getitnow:(Void -> Int), unused:(Float -> Int -> Float)):Void
    {
        trace(getit(getitnow()));
    }
    public function new()
    {
    }
}");
        }
     
        [Test]
        public void TypeStatics()
        {
            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            StringBuilder.DateTime.MythicalField = 4;
            trace(int.MaxValue);
            trace(int.MinValue);
            string s = ""123"";
            trace(int.Parse(s) + 1);
            float.Parse(s);
            double.Parse(s);
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"
import system.text.StringBuilder;

class Utilities
{
    public static function SomeFunction():Void
    {
        StringBuilder.DateTime.MythicalField = 4;
        trace(2147483647);
        trace(-2147483648);
        var s:String = ""123"";
        trace(Std.parseInt(s) + 1);
        Std.parseFloat(s);
        Std.parseFloat(s);
    }
    public function new()
    {
    }
}");
        }

        [Test]
        public void DictionaryAndHashSet()
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
            Dictionary<int, int> dict = new Dictionary<int, int>();
            dict.Add(4, 3);
            trace(dict[4]);
            trace(dict.ContainsKey(8));
            dict.Remove(4);
            foreach(int key in dict.Keys)
                trace(key);
            foreach(int val in dict.Values)
                trace(val);
            
            HashSet<int> hash = new HashSet<int>();
            hash.Add(999);
            trace(hash.Contains(999));
            hash.Remove(999);
            trace(hash.Contains(999));
            foreach(int hashItem in hash)
                trace(hashItem);
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"
import system.collections.generic.CSDictionary;
import system.collections.generic.HashSet;

class Utilities
{
    public static function SomeFunction():Void
    {
        var dict:CSDictionary<Int, Int> = new CSDictionary<Int, Int>();
        dict.Add(4, 3);
        trace(dict.GetValue(4));
        trace(dict.ContainsKey(8));
        dict.Remove(4);
        for (key in dict.Keys)
        {
            trace(key);
        }
        for (val in dict.Values)
        {
            trace(val);
        }
        
        var hash:HashSet<Int> = new HashSet<Int>();
        hash.Add(999);
        trace(hash.Contains(999));
        hash.Remove(999);
        trace(hash.Contains(999));
        for (hashItem in hash.Values())
        {
            trace(hashItem);
        }
    }
    public function new()
    {
    }
}");
        }

        [Test]
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
            trace(nullableInt.HasValue);
            int? withValue = new Nullable<int>(8);
            trace(withValue.Value);
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"
import system.Nullable_Int;

class Utilities
{
    public static function SomeFunction():Void
    {
        var nullableInt:Nullable_Int = new Nullable_Int();
        trace(nullableInt.HasValue);
        var withValue:Nullable_Int = new Nullable_Int(8);
        trace(withValue.Value);
    }
    public function new()
    {
    }
}");
        }

        [Test]
        public void Enums()
        {

            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, new string[] { @"
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
            MostlyNumbered f = MostlyNumbered.One;
            UnNumbered[] arr = new UnNumbered[] { UnNumbered.One, UnNumbered.Two, UnNumbered.Three };
            int i = (int)f;
        }
    }
}", @"
using Blargh;

namespace OtherNamespace
{
    class OtherClass
    {
        public OtherClass()
        {
            if (SomethingElse.Foo() == MostlyNumbered.One)
            {
            }
        }
    }
}" }, new string[] { @"
package blargh;
" + Program.StandardImports + @"
class MostlyNumbered
{
    public static var One:Int = 1;
    public static var Two:Int = 2;
    public static var Three:Int = 3;
    public static var Unnumbered:Int = 4;
    public static var SomethingElse:Int = 50;
}", @"
package blargh;
" + Program.StandardImports + @"
class UnNumbered
{
	public static var One:Int = 1; 
    public static var Two:Int = 2;
    public static var Three:Int = 3;
}", @"
package blargh;
" + Program.StandardImports + @"
class Clazz
{
    public static function Methodz():Void
    {
        var f:Int = MostlyNumbered.One;
        var arr:Array<Int> = [ UnNumbered.One, UnNumbered.Two, UnNumbered.Three ];
        var i:Int = f;
    }
    public function new()
    {
    }
}", @"
package othernamespace;
" + Program.StandardImports + @"
import blargh.MostlyNumbered;

class OtherClass
{
    public function new()
    {
        if (SomethingElse.Foo() == MostlyNumbered.One)
        {
        }
    }
}

"});
        }
        [Test]
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
                case ""NotMe"": trace(4); break;
                case ""Box"": trace(4); break;
                case ""Blah"": trace(3); break;
                default: throw new InvalidOperationException();
            }
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"
import system.InvalidOperationException;

class Utilities
{
    public static function SomeFunction():Void
    {
        var s:String = ""Blah"";
        switch (s)
        {
            case ""NotMe"":
                trace(4);
            case ""Box"": 
                trace(4); 
            case ""Blah"": 
                trace(3); 
            default: 
                throw new InvalidOperationException();
        }
    }
    public function new()
    {
    }
}");
        }
        [Test]
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
            trace(e.First());
            trace(e.FirstWhere(o => o == 1));
            trace(e.ElementAt(2));
            trace(e.Last());
            trace(e.Count());
            trace(e.Where(o => o > 0).Count() + 2);
            trace(e.CountWhere(o => true) + 2);

            Dictionary<int, int> dict = e.ToDictionary(o => o, o => 555);
            e.OfType<int>();
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"
import system.collections.generic.CSDictionary;
import system.linq.Linq;

class Utilities
{
    public static function SomeFunction():Void
    {
        var e:Array<Int> = [ 0, 1, 2, 3 ];
        trace(Linq.First(e));
		
        trace(Linq.FirstWhere(e, function (o)
        {
            return o == 1;
        } ));
        trace(Linq.ElementAt(e, 2));
        trace(Linq.Last(e));
        trace(Linq.Count(e));
        trace(Linq.Count(Linq.Where(e, function (o)
        {
            return o > 0;
        } )) + 2);
        trace(Linq.CountWhere(e, function (o)
        {
            return true;
        } ) + 2);
        var dict:CSDictionary<Int, Int> = Linq.ToDictionary(e, function (o)
        {
            return o;
        } , function (o)
        {
            return 555;
        } );
        Linq.OfType(e, Int);
    }
    public function new()
    {
    }
}");
        }

        [Test]
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
            OverOne(3, ""Blah"");
        }

        public static void OverOne(int param)
        {
            OverOne(3, ""Blah"");
        }

        public static void OverOne(int param, string prm)
        {
            trace(param + prm);
        }

        public static int OverTwo(int prm)
        {
            return prm;
        }
        public static int OverTwo()
        {
            return OverTwo(18);
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"

class Utilities
{
    public static function OverOne(param:Int = 3, prm:String = ""Blah""):Void
    {
        trace(param + prm);
    }
    public static function OverTwo(prm:Int = 18):Int
    {
        return prm;
    }
    public function new()
    {
    }
}");
        }


        [Test]
        public void IsAndAs()
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
            var list = new List<int>();
            if (s is string)
                trace(""Yes"");
            if (list is List<int>)
                trace(""Yes"");

//            object o = s;
//            string sss = o as string;
//            trace(sss);
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        var s:String = ""Blah"";
        var list:Array<Int> = new Array<Int>();
        if (Std.is(s, String))
        {
            trace(""Yes"");
        }
        if (Std.is(list, Array))
        {
            trace(""Yes"");
        }
    }
    public function new()
    {
    }
}");
        }
        

        [Test]
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
            trace(""TopLevel::VirtualMethod"");
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
            trace(""Derived::AbstractMethod"");
        }

        public override string AbstractProperty
        {
            get { return ""Derived::AbstractProperty""; }
        }

        public override void VirtualMethod()
        {
            base.VirtualMethod();
            trace(""Derived::VirtualMethod"");
        }

        public override string VirtualProperty
        {
            get
            {
                return base.get_VirtualProperty() + ""Derived:VirtualProperty"";
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
" + Program.StandardImports + @"
class TopLevel
{
	public var AbstractProperty(get_AbstractProperty, never):String;
	
    public function get_AbstractProperty():String
    {
    	throw new Exception(""Abstract item called"");
		return null;
    }

    public var VirtualProperty(get_VirtualProperty, never):String;
    public function get_VirtualProperty():String
    {
        return ""TopLevel::VirtualProperty"";
    }

    public function AbstractMethod():Void
    {
    	throw new Exception(""Abstract item called"");
    }
    public function VirtualMethod():Void
    {
        trace(""TopLevel::VirtualMethod"");
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
" + Program.StandardImports + @"
class Derived extends TopLevel
{
    override public function get_AbstractProperty():String
    {
        return ""Derived::AbstractProperty"";
    }
    override public function get_VirtualProperty():String
    {
        return super.get_VirtualProperty() + ""Derived:VirtualProperty"";
    }

    override public function AbstractMethod():Void
    {
        trace(""Derived::AbstractMethod"");
    }
    override public function VirtualMethod():Void
    {
        super.VirtualMethod();
        trace(""Derived::VirtualMethod"");
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


        [Test]
        public void FieldsAndProperties()
        {
            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

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
            set { trace(value); }
        }

        public Int GetOnly
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

        static Box()
        {
            trace(""cctor"");
        }

        public Box()
        {
            trace(""ctor"");
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"
import system.text.StringBuilder;

class Box
{
    private var _width:Float;
    public var IsRectangular:Bool;
    public var Characters:Array<Int>;
    public static var StaticField:StringBuilder;
    public static inline var ConstInt:Int = 24;
    public static inline var StaticReadonlyInt:Int = 5;
    public static inline var WithQuoteMiddle:String = ""before\""after"";
    public static inline var WithQuoteStart:String = ""\""after"";
    public var MultipleOne:Int;
    public var MultipleTwo:Int;
    public var ReadonlyInt:Int;

    public var Width(get_Width, set_Width):Float;
    public function get_Width():Float
    {
        return _width;
    }
    public function set_Width(value:Float):Float
    {
        _width = value;
		return 0;
    }
    public var SetOnly(never, set_SetOnly):Float;
    public function set_SetOnly(value:Float):Float
    {
        trace(value);
		return 0;
    }
    public var GetOnly(get_GetOnly, never):Int;
    public function get_GetOnly():Int
    {
        return 4;
    }

    public static function cctor():Void
    {
        StaticField = new StringBuilder();
        trace(""cctor"");
    }

	public function new()
	{
		IsRectangular = true;
		Characters = [ 97, 98 ];
        ReadonlyInt = 3;
        trace(""ctor"");
	}
}");
        }

        

        [Test]
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
            trace(""Implementation"");
        }
    }
}",
  new string[] { @"
package blargh;
" + Program.StandardImports + @"

interface ITesting
{
    function Poke():Void;
}",
  @"
package blargh;
" + Program.StandardImports + @"

class Pokable implements ITesting
{
    public function Poke():Void
    {
        trace(""Implementation"");
    }
    public function new()
    {
    }
}"});
        }

        [Test]
        public void TryCatchThrow()
        {
            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            trace(""Before try"");
            try
            {
                trace(""In try"");
            }
            catch (Exception ex)
            {
                trace(""In catch"");
            }

            try
            {
                trace(""Try without finally"");
            }
            catch (IOException ex)
            {
                trace(""In second catch"");
            }

            try
            {
                trace(""Try in parameterless catch"");
            }
            catch
            {
                trace(""In parameterless catch"");
            }

            throw new InvalidOperationException(StringBuilder.MythicalField);
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"
import system.Exception;
import system.InvalidOperationException;
import system.text.StringBuilder;

class Utilities
{
    public static function SomeFunction():Void
    {
        trace(""Before try"");
        try
        {
            trace(""In try"");
        }
        catch (ex:Exception)
        {
            trace(""In catch"");
        }
        try
        {
            trace(""Try without finally"");
        }
        catch (ex:IOException)
        {
            trace(""In second catch"");
        }
        try
        {
            trace(""Try in parameterless catch"");
        }
        catch (__ex:Dynamic)
        {
            trace(""In parameterless catch"");
        }

        throw new InvalidOperationException(StringBuilder.MythicalField);
    }
    public function new()
    {
    }
}");
        }

        

        [Test]
        public void Generics()
        {
            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

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
                effect(o);
            return array;
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"

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
            effect(o);
        }
        return array;
    }
    public function new()
    {
    }
}");
        }

        [Test]
        public void Objects()
        {
            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            var queue = new Queue<int>(10);
            queue.Enqueue(4);
            queue.Enqueue(2);
            trace(queue.Dequeue());
            queue.Clear();
    
            var list = new List<string>(3);
            list.Add(""Three"");
            list.RemoveAt(0);
            list.Insert(4, ""Seven"");

            var stack = new Stack<bool>();
            stack.Push(true);
            stack.Push(false);
            Math.Max(stack.Pop(), stack.Pop();
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        var queue:Array<Int> = new Array<Int>();
        queue.push(4);
        queue.push(2);
        trace(queue.shift());
        queue.splice(0, queue.length);

        var list:Array<String> = new Array<String>();
        list.push(""Three"");
        list.splice(0, 1);
        list.insert(4, ""Seven"");

        var stack:Array<Bool> = new Array<Bool>();
        stack.push(true);
        stack.push(false);
        Math.max(stack.pop(), stack.pop());
    }
    public function new()
    {
    }
}");
        }

        [Test]
        public void Lambda()
        {
            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            Func<int, int> f1 = x => x + 5;
            trace(f1(3));
            Func<int, int> f2 = x => { return x + 6; };
            trace(f2(3));

            List<Action> actions = new List<Action>();
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        var f1:(Int -> Int) = function (x:Int):Int 
        { 
            return x + 5; 
        } ;
        trace(f1(3));
        var f2:(Int -> Int) = function (x:Int):Int 
        { 
            return x + 6; 
        } ;
        trace(f2(3));
        var actions:Array<(Void -> Void)> = new Array<(Void -> Void)>();
    }
    public function new()
    {
    }
}");
        }

        [Test]
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
" + Program.StandardImports + @"

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
        Foo(function ()
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
        [Test]
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
                trace(""hi"");
                break;
            }

            for (int i=0;i<50;i++)
                trace(i);

            do
            {
                trace(""Dowhile"");
            }
            while (false);
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        while (true)
        {
            trace(""hi"");
            break;
        }

        { //for
            var i:Int = 0;
            while (i < 50)
            {
                trace(i);
                i++;
            }
        } //end for
        do
        {
            trace(""Dowhile"");
        }
        while (false);
    }
    public function new()
    {
    }
}");
        }

        [Test]
        public void ReplaceTypeWithAttribute()
        {
            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public class Foo
    {
        [Cs2Hx(ReplaceWithType = ""bar.Baz"")]
        public object Obj;
    }
}", @"
package blargh;
" + Program.StandardImports + @"

class Foo
{
    public var Obj:bar.Baz;
    public function new()
    {
    }
}");
        }

        [Test]
        public void CastsWithAs()
        {
            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            var z = one.As<Two>();
            one.two.three.As<Four>().five;
            one.two().As<Three>().four;
            var a = (DateTime)z;
            object o = new object();
            var b = (DateTime)o;
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"
import system.DateTime;

class Utilities
{
    public static function SomeFunction():Void
    {
        var z = cast(one, Two);
        cast(one.two.three, Four).five;
        cast(one.two(), Three).four;
        var a:DateTime = cast(z, DateTime);
        var o:Dynamic = new Dynamic();
        var b:DateTime = o;
    }
    public function new()
    {
    }
}");
        }

        [Test]
        public void ArrayAndForEach()
        {
            TestFramework.TestCode(MethodInfo.GetCurrentMethod().Name, @"
using System;

namespace Blargh
{
    public static class Utilities
    {
        public static void SomeFunction()
        {
            string[] ar = new string[] { 1, 2, 3 };

            foreach(int i in ar)
                trace(i);

            trace(ar[1]);
            trace(ar.Length);
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        var ar:Array<String> = [ 1, 2, 3 ];
        for (i in ar)
        {
        	trace(i);
        }
        trace(ar[1]);
        trace(ar.length);
    }
    public function new()
    {
    }
}");
        }

        [Test]
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
            trace(""I'm in one!"");
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
            trace(""I'm in Two!"");
        }
    }
    public function new()
    {
    }
}"
}, @"
package blargh;
" + Program.StandardImports + @"

class Utilities
{
    public function FunFromOne():Void
    {
        trace(""I'm in one!"");
    }
    public function FunFromTwo():Void
    {
        trace(""I'm in Two!"");
    }
    public function new()
    {
    }
}");
        }

        [Test]
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
            string s = @""500"";
            trace(s.IndexOf(""0""));
            trace(s2.IndexOf(""0""));

            foreach(string s3 in new string[] { ""Hello"" })
                s3.Substring(4, 5);

            int i = 4;
            string si = i.ToString();
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"
class Utilities
{
    public static function SomeFunction(s2:String):Void
    {
        var s:String = ""500"";
        trace(s.indexOf(""0""));
        trace(s2.indexOf(""0""));

        for (s3 in [ ""Hello"" ])
        {
            s3.substr(4, 5);
        }
        var i:Int = 4;
        var si:String = Std.string(i);
    }
    public function new()
    {
    }
}");
        }

        [Test]
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
            trace(""false "" + i.IsFour());
            i++;
            i += 6;
            bool b = i.IsFour();
            trace(""true "" + b);
            Blargh.Utilities.IsFour(5);
        }

        public static bool IsFour(this int i)
        {
            return i == 4;
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        var i:Int = -3;
        trace(""false "" + blargh.Utilities.IsFour(i));
        i++;
        i += 6;
        var b:Bool = blargh.Utilities.IsFour(i);
        trace(""true "" + b);
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

        [Test]
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
            var s = string.Join(asdf, "";"");
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"

class Foo
{
    public function new()
    {
        var s = Cs2Hx.Join(asdf, "";"");
    }
}");
        }

        [Test]
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
            trace(""Hello, World!"");
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"

class Utilities
{
    public static function SomeFunction():Void
    {
        trace(""Hello, World!"");
    }
    public function new()
    {
    }
}");
        }
        [Test]
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

            trace(myNum == 999 ? ""One"" : ""Two"");
        }
    }
}", @"
package blargh;
" + Program.StandardImports + @"
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

        trace(myNum == 999 ? ""One"" : ""Two"");
    }
    public function new()
    {
    }
}");
        }
    }
}
