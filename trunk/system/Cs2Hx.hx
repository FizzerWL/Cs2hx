package system;
import system.Exception;

class Cs2Hx
{
	public static function Join(sep:String, a:Array<String>):String
	{
		return a.join(sep);
	}
	
	private static function MakeMap(str:String):IntHash<Bool>
	{
		var hash:IntHash<Bool> = new IntHash<Bool>();
		
		var i = 0;
		while (i < str.length)
			hash.set(str.charCodeAt(i++), true);
		return hash;
	}
	
	static var charOrDigitMap:IntHash<Bool>;
	public static function IsLetterOrDigit(ch:Int):Bool
	{
		if (charOrDigitMap == null)
			charOrDigitMap = MakeMap("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");
		
		return charOrDigitMap.exists(ch);
	}

	public static function IndexOf<T>(a:Array<T>, item:T):Int
	{
		for (i in 0...a.length)
			if (a[i] == item)
				return i;
		return -1;
	}
	
	static var digitMap:IntHash<Bool>;
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
}