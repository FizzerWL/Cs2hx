package system;
import system.Exception;

class Cs2Hx
{
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
	
	public static inline function SortInts(f:Int, s:Int):Int
	{
		return f - s;
	}
	
	public static inline function SortFloats(f:Float, s:Float):Int
	{
		return Std.int(f - s);
	}
}