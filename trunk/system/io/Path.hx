package system.io;
import haxe.io.Bytes;
import system.NotImplementedException;

class Path
{

	public function new() 
	{
		
	}
	
	public static function Combine_String_String(p1:String, p2:String):String
	{
		return throw new NotImplementedException();
	}
	public static function Combine_String_String_String(p1:String, p2:String, p3:String):String
	{
		return throw new NotImplementedException();
	}
	public static function Combine_String_String_String_String(p1:String, p2:String, p3:String, p4:String):String
	{
		return throw new NotImplementedException();
	}
	public static function GetDirectoryName(path:String):String
	{
		return throw new NotImplementedException();
	}
	
	public static function GetFileName(path:String):String
	{
		return throw new NotImplementedException();
	}
	
	public static inline var DirectorySeparatorChar:String = "/";
	
	public static function GetTempFileName():String
	{
		return throw new NotImplementedException();
	}
	
}