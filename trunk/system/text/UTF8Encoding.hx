package system.text;

import haxe.io.Bytes;
import system.Exception;
import system.NotImplementedException;

class UTF8Encoding extends Encoding
{
	public function GetString(b:Bytes):String
	{
		#if js
		var decoder = js.Syntax.code("new TextDecoder()");
		var arr = haxe.io.UInt8Array.fromBytes(b);
        return decoder.decode(arr);
		#else
		throw new NotImplementedException();
		#end	
	}
	
	public function GetBytes_String(str:String):Bytes
	{
		#if js
		var encoder = js.Syntax.code("new TextEncoder()");
        return haxe.io.UInt8Array.fromArray(encoder.encode(str)).view.buffer;
		#else
		throw new NotImplementedException();
		#end	
		
	}
	
	public function new() 
	{ 
		super();
	}
	
	
	public static var UTF8(get, never):UTF8Encoding;
	
	private static function get_UTF8():UTF8Encoding
	{
		return new UTF8Encoding();
	}
	
}