package system;
import haxe.crypto.Base64;
import haxe.io.Bytes;

class Convert
{

	public function new() 
	{
		
	}
	
	public static function ToByte_String_Int32(s:String, base:Int):Int
	{
		return ToInt32_String_Int32(s, base);
	}
	public static function ToInt32_String_Int32(s:String, base:Int):Int
	{
		if (base == 16)
		{
			return Std.parseInt("0x" + s);
		}
		else
			return throw new NotImplementedException("ToInt32_String_Int32 base=" + base);
	}
	
	public static function ToBase64String(bytes:Bytes):String
	{
		return Base64.encode(bytes);
	}
	
	public static function FromBase64String(str:String):Bytes
	{
		return Base64.decode(str);
	}
	
	

	public static function toString(i:Int, base:Int):String
	{
		return throw new NotImplementedException("convert.base");
	}
}