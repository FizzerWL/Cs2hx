package system.text;

import haxe.io.Bytes;
import system.Exception;
import system.NotImplementedException;

class UTF8Encoding 
{
	public function GetString(b:Bytes):String
	{
		return throw new NotImplementedException();
	}
	
	public function GetBytes_String(str:String):Bytes
	{
		/*
		//This works in flash on Windows and Mac, but not Linux.  Commented out until a solution can be found that works everywhere.
		var r:ByteArray = new ByteArray();
		r.writeMultiByte(str, "utf-8");
		return Bytes.ofData(r);
		*/
		return throw new NotImplementedException();
	}
	
	public function new() 
	{ 
	}
	
	
	public static var UTF8(get, never):UTF8Encoding;
	
	private static function get_UTF8():UTF8Encoding
	{
		return new UTF8Encoding();
	}
	
}