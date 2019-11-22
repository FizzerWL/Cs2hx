package system;
using StringTools;
import system.collections.generic.IComparer.IComparer;
import system.collections.generic.IEnumerable;
import system.Exception;
import system.text.StringBuilder;

class Cs2Hx
{
	public static function ParseOrZero(str:String):Int
	{
		#if flash
		return Std.parseInt(str);
		#else
		var r = Std.parseInt(str);
		return r == null ? 0 : r;
		#end
	}
	public static inline function GuidParse(s:String):String
	{
		return s;
	}
	
	public static function Remove_Int32_Int32(s:String, startIndex:Int, count:Int):String
	{
		return s.substr(0, startIndex) + s.substr(startIndex + count - 1);
	}
	
	public static function FindIndex<T>(a:Array<T>, match:T->Bool):Int
	{
		var i:Int = 0;
		for (e in a)
		{
			if (match(e))
				return i;
			i++;
		}
		return -1;
	}
	
	public static function RemoveAll<T>(a:Array<T>, match:T->Bool):Int
	{
		var numRemoved = 0;
		var i = a.length - 1;
		while (i >= 0)
		{
			var e = a[i];
			if (match(e))
			{
				a.splice(i, 1);
				numRemoved++;
			}
			else
				i--;
		}
		
		return numRemoved;
	}
	
	public static function AddRange<T, K:T>(b:Array<T>, a:Array<K>):Void 
	{
		for (e in a)
			b.push(e);
	}
	public static function InsertRange<T>(b:Array<T>, index:Int, a:Array<T>):Void
	{
		for (e in a)
			b.insert(index++, e);
	}
	public static function ForEach<T>(a:Array<T>, func:T->Void):Void
	{
		for (e in a)
			func(e);
	}
	
	public static inline function GetType(c:Dynamic):TypeCS
	{
		return new system.TypeCS(c);
	}
	
	
	public static function IndexOfAny__Int32(s:String, chars:Array<Int>, startat:Int):Int
	{
		for (i in startat...s.length)
		{
			var ec = s.charCodeAt(i);
			for (c in chars)
				if (ec == c)
					return i;
		}
		
		return -1;
	}
	
	public static function IsNullOrWhiteSpace(s:String):Bool
	{
		return s == null || Trim(s).length == 0;
	}
	
	public static function Replace(s:String, c1:Int, c2:Int):String
	{
		return s.replace(String.fromCharCode(c1), String.fromCharCode(c2));
	}
	
	public static function ToCharArray(s:String):Array<Int>
	{
		var ret = new Array<Int>();
		
		for (i in 0...s.length)
			ret.push(s.charCodeAt(i));
		return ret;
	}
	
	public static inline function Coalesce<T>(a:T, b:T):T
	{
		return a == null ? b : a;
	}
	
	public static inline function Clear<T>(a:Array<T>):Void
	{
		a.splice(0, a.length);
	}
	
	public static inline function IsNaN(a:Float):Bool
	{
		return Math.isNaN(a);
	}
	
	public static function Equals_String_StringComparison(str1:String, str2:String, type:Int):Bool
	{
		if (type == StringComparison.OrdinalIgnoreCase)
			return str1 != null && str2 != null && str1.toLowerCase() == str2.toLowerCase();
		else
			return str1 == str2;
	}
	public static function EndsWith_String_StringComparison(str:String, endsWith:String, type:Int):Bool
	{
		return throw new NotImplementedException();
	}

	public static inline function Contains<T>(a:Array<T>, item:T):Bool
	{
		return IndexOf(a, item) != -1;
	}

	public static inline function StringContains(haystack:String, needle:String):Bool
	{
		return haystack.indexOf(needle) != -1;
	}
	
	public static inline function IndexOfChar(s:String, c:Int, startAt:Int = 0):Int
	{
		return s.indexOf(String.fromCharCode(c), startAt);
	}


	public static function ParseBool(str:String):Bool
	{
		switch (str.toLowerCase())
		{
			case "true": return true;
			case "false": return false;
			default:
				throw new Exception("parseBool passed " + str);
		}
	}
	
	public static inline function ToArray<T>(a:Array<T>):Array<T>
	{
		return a;
	}
	
	public static inline function Split__StringSplitOptions(s:String, chars:Array<Int>, options:Int):Array<String>
	{
		return Split(s, chars, options);
	}
	
	public static function Split(s:String, chars:Array<Int>, options:Int = 0):Array<String>
	{
		var charString:String = "";
		
		for (c in chars)
			charString += String.fromCharCode(c);
			
		var split = s.split(charString);
		
		if (options == StringSplitOptions.RemoveEmptyEntries)
		{
			var ret = new Array<String>();
			for (e in split)
				if (!IsNullOrEmpty(e))
					ret.push(e);
					
			return ret;
		}
		else
			return split;
	}
	
	public static inline function Join(sep:String, a:Array<String>):String
	{
		return a.join(sep);
	}
	
	private static inline function MakeMap(str:String):Map<Int, Bool>
	{
		var hash:Map<Int, Bool> = new Map<Int, Bool>();
		
		var i = 0;
		while (i < str.length)
			hash.set(str.charCodeAt(i++), true);
		return hash;
	}
	
	static var charOrDigitMap:Map<Int, Bool>;
	public static function IsLetterOrDigit(ch:Int):Bool  //TODO: This differs from char.IsLetterOrDigit since it only recognizes ascii characters.
	{
		if (charOrDigitMap == null)
			charOrDigitMap = MakeMap("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");
		
		return charOrDigitMap.exists(ch);
	}

	public static function IsLetterOrDigitAtIndex(str:String, index:Int):Bool
	{
		return IsLetterOrDigit(str.charCodeAt(index));
	}

	public static function IndexOf<T>(a:Array<T>, item:T):Int
	{
		for (i in 0...a.length)
			if (a[i] == item)
				return i;
		return -1;
	}
	public static function LastIndexOf<T>(a:Array<T>, item:T):Int
	{
		for (i in 0...a.length)
			if (a[a.length - i - 1] == item)
				return a.length - i - 1;
		return -1;
	}
	
	static var digitMap:Map<Int, Bool>;
	public static function IsDigit(ch:Int):Bool
	{
		if (digitMap == null)
			digitMap = MakeMap("0123456789");
			
		return digitMap.exists(ch);
	}
	
	public static inline function IsInfinity(f:Float):Bool
	{
		return !Math.isFinite(f);
	}
	
	private static var _identity:Int = 0;
	public static function Hash(o:Dynamic):String
	{
		if (o == null)
			return "";
		if (Std.is(o, String))
			return o;
		if (Std.is(o, Int))
			return Std.string(o);
		
		#if flash
		return Std.string(o);
		#else
		//For now, every object gets a unique ID.  TODO: Wire up the GetHashCode function and use it here.
		if (o.__csid__)
			return o.__csid__;
		var newID = Std.string(++_identity);
		o.__csid__ = newID;
		return newID;
		#end
	}
	public static inline function MathMin(f:Int, s:Int):Int
	{
		if (f > s)
			return s;
		else
			return f;
	}
	
	public static inline function IsNullOrEmpty(str:String):Bool
	{
		return str == null || str.length == 0;
	}
	
	public static inline function CharToString(c:Int):String
	{
		return String.fromCharCode(c);
	}
	
	public static inline function StartsWith(str1:String, str2:String):Bool
	{
		return str1.substr(0, str2.length) == str2;
	}
	public static function StartsWith_String_StringComparison(str1:String, str2:String, comp:Int):Bool
	{
		return throw new NotImplementedException();
	}
	
	
	
	public static inline function EndsWith(str1:String, str2:String):Bool
	{
		return str1.substr(str1.length - str2.length) == str2;
	}
	
	public static inline function ByteToHex(i:Int):String
	{
		return CharToHex(Std.int(i / 16)) + CharToHex(i % 16);
	}
	
	public static inline function AbsInt(i:Int):Int
	{
		return i >= 0 ? i : -i;
	}

	public static function CharToHex(i:Int):String
	{
		switch (i)
		{
			case 10: return "A";
			case 11: return "B";
			case 12: return "C";
			case 13: return "D";
			case 14: return "E";
			case 15: return "F";
			default:
				if (i < 0 || i >= 16)
					throw new Exception("ToHex out of range");
				return Std.string(i);
		}
	}
	
	public static function Trim_(str:String, chars:Array<Int>):String
	{
		if (str.length == 0)
			return str;

		var i:Int = 0;
		while (IndexOf(chars, str.charCodeAt(i)) != -1 && i < str.length)
			i++;
			
		var e:Int = str.length - 1;
		while (IndexOf(chars, str.charCodeAt(e)) != -1  && e > 0) 
			e--;

		if (e < 0)
			return "";

		return str.substr(i, e - i + 1);
	}
	
	public static function Trim(str:String):String
	{
		if (str.length == 0)
			return str;
			
		var i:Int = 0;
		while (str.charCodeAt(i) < 33 && i < str.length)
			i++;
			
		var e:Int = str.length - 1;
		while (str.charCodeAt(e) < 33 && e > 0) 
			e--;

		if (e < 0)
			return "";

		return str.substr(i, e - i + 1);
	}
	
	public static function TrimEnd(str:String, chars:Array<Int> = null):String
	{
		if (chars == null)
			return str.rtrim();
		
		return throw new NotImplementedException();
			
	}
	public static function TrimStart(str:String, chars:Array<Int> = null):String
	{
		if (chars == null)
			return str.ltrim();
		var i = 0;
		while (i < str.length)
		{
			var c = str.charCodeAt(i);
			for (ch in chars)
				if (c == ch)
				{
					i++;
					continue;
				}
			
			break;
		}
		
		return str.substr(i);
	}
	
	
	public static inline function SortInts(f:Int, s:Int):Int
	{
		return f - s;
	}
	
	public static inline function SortFloats(f:Float, s:Float):Int
	{
		return Std.int(f - s);
	}
	
	public static function Sort_Int32_Int32_IComparer<T>(array:Array<T>, startAt:Int, len:Int, comp:IComparer<T>):Void
	{
		var tmp = new Array<T>();
		
		for (i in startAt...(len + startAt))
			tmp.push(array[i]);
			
		tmp.sort(function (a:T, b:T):Int { return comp.Compare(a, b); } );
		
		for (i in 0...tmp.length)
			array[i + startAt] = tmp[i];
	}
	
	public static function NewGuid():String
	{
		//This is not a real guid generation algorithm.  This just generates 16 random bytes, which is enough for some uses.
		var ret:String = "";
		
		for(i in 0...32)
			ret += Cs2Hx.CharToHex(Std.random(16));
		
		if (ret.length != 32)
			throw new Exception();
		return ret;
	}
	
	public static function EmptyGuid():String
	{
		return "00000000000000000000000000000000";
	}
	
	public static function IsUpper(char:Int):Bool
	{
		return throw new NotImplementedException();
	}
	public static function ToLower(char:Int):Int
	{
		return throw new NotImplementedException();
	}
	
	public static function TryParseInt(s:String, out:CsRef<Int>):Bool
	{
		var i = Std.parseInt(s);
		
		if (i == null)
			return false;
		if (i == 0 && s != "0")
			return false;
		
		out.Value = i;
		return true;
	}
	
	public static function TryParseFloat(s:String, out:CsRef<Float>):Bool
	{
		var i:Float = Std.parseFloat(s);
		
		if (Math.isNaN(i))
			return false;
			
		out.Value = i;
		return true;
	}
	public static function TryParseBool(s:String, out:CsRef<Bool>):Bool
	{
		switch (s.toLowerCase())
		{
			case "true":
				out.Value = true;
				return true;
			case "false":
				out.Value = false;
				return true;
			default:
				return false;
		}
	}
	
	public static inline function GetEnumeratorNullCheck<T>(obj:IEnumerable<T>):Array<T>
	{
		if (obj == null)
			return null;
		else
			return obj.GetEnumerator();
	}

	public static function NewString(ch:Int, repeat:Int):String
	{
		var buf = new StringBuilder();
		for (i in 0...repeat)
			buf.Append_Char(ch);
		return buf.toString();
	}
	public static inline function BoolCompare(b1:Bool, b2:Bool):Bool
	{
		#if js
		//js target seems to leave things undefined, which means comparing booleans with == won't always give the correct answer
		return !b1 == !b2;
		#else
		return b1 == b2;
		#end
	}

	public static inline function NullCheck(str:String):String
	{
		return str == null ? "" : str;
	}
}