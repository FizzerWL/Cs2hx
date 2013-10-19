package system.io;
import haxe.io.Bytes;

class Stream
{

	public function new() 
	{
		
	}
	
	public function Dispose():Void
	{
	}
	public function Write(buf:Bytes, startAt:Int, len:Int):Void
	{
		throw new NotImplementedException();
	}
	public function Read(buf:Bytes, offset:Int, count:Int):Int
	{
		return throw new NotImplementedException();
	}
	
	
}