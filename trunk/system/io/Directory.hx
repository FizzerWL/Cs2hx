package system.io;
import system.NotImplementedException;

class Directory
{

	public function new() 
	{
		
	}
	
	public static function Exists(path:String):Bool
	{
		return throw new NotImplementedException();
	}
	
	public static function CreateDirectory(path:String):Void
	{
		throw new NotImplementedException();
	}
	public static function GetFiles_String_String_SearchOption(path:String, pattern:String, option:Int):Array<String>
	{
		return throw new NotImplementedException();
	}
	public static function GetFiles_String_String(path:String, pattern:String):Array<String>
	{
		return throw new NotImplementedException();
	}
	
	public static function GetDirectories(path:String):Array<String>
	{
		return throw new NotImplementedException();
	}
	
	public static function GetDirectories_String_String_SearchOption(path:String, searchPattern:String, option:Int):Array<String>
	{
		return throw new NotImplementedException();
	}
	
	public static function Delete_String_Boolean(path:String, recur:Bool):Void
	{
		throw new NotImplementedException();
	}
}