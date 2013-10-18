package system;
import system.Exception;

class Cs2Hx
{
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
	
	

	public static inline function Contains<T>(a:Array<T>, item:T):Bool
	{
		return IndexOf(a, item) != -1;
	}

	public static inline function StringContains(haystack:String, needle:String):Bool
	{
		return haystack.indexOf(needle) != -1;
	}
	
	public static inline function IndexOfChar(s:String, c:Int):Int
	{
		return s.indexOf(String.fromCharCode(c));
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
	
	public static inline function Split(s:String, chars:Array<Int>):Array<String>
	{
		var charString:String = "";
		
		for (c in chars)
			charString += String.fromCharCode(c);
		return s.split(charString);
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
	
	public static inline function Hash(o:Dynamic):String
	{
		return Std.string(o);
	}
	
	public static inline function IsNullOrEmpty(str:String):Bool
	{
		return str == null || str.length == 0;
	}
	
	public static inline function StartsWith(str1:String, str2:String):Bool
	{
		return str1.substr(0, str2.length) == str2;
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
	
	public static function Trim(str:String):String
	{
		var i:Int = 0;
		while (str.charCodeAt(i) < 33)
			i++;
			
		var e:Int = str.length - 1;
		while (str.charCodeAt(e) < 33) 
			e--;

		if (e < 0)
			return "";

		return str.substr(i, e - i + 1);
	}
	
	
	public static inline function SortInts(f:Int, s:Int):Int
	{
		return f - s;
	}
	
	public static inline function SortFloats(f:Float, s:Float):Int
	{
		return Std.int(f - s);
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
	
	public static inline function IsUpper(char:Int):Bool
	{
		return throw new NotImplementedException();
	}
	public static inline function ToLower(char:Int):Bool
	{
		return throw new NotImplementedException();
	}
}