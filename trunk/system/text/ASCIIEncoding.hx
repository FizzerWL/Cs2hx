package system.text;
import haxe.io.Bytes;
import system.NotImplementedException;

class ASCIIEncoding extends Encoding
{

	public function new() 
	{
		super();
	}
	
	public function GetBytes_String(str:String):Bytes
	{
		return throw new NotImplementedException();
	}
	
}