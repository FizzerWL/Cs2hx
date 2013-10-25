package system.io;
import haxe.io.Bytes;
import system.NotImplementedException;

class MemoryStream extends Stream
{
	public function new(data:Bytes = null) 
	{
		super(data);
	}
	
	
	public function Seek(offset:Int, origin:Int):Void
	{
		throw new NotImplementedException();
	}
	
}