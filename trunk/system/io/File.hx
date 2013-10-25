package system.io;
import haxe.io.Bytes;
import system.NotImplementedException;

class File
{

	public function new() 
	{
		
	}
	
	public static function Exists(path:String):Bool
	{
		return throw new NotImplementedException();
	}
	
	public static function AppendAllText(path:String, contents:String):Void
	{
		throw new NotImplementedException();
	}
	public static function OpenRead(path:String):FileStream
	{
		return throw new NotImplementedException();
	}
	public static function ReadAllBytes(path:String):Bytes
	{
		return throw new NotImplementedException();
	}
}