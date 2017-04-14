package system.io;
import haxe.io.Bytes;
import system.NotImplementedException;

class Stream
{
	var _bytes:Bytes;

	public function new(bytes:Bytes) 
	{
		_bytes = bytes;
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
	

	public function Close():Void
	{
		throw new NotImplementedException();
	}
	
	//haxe has limited stream reading functionality, so we usually just read it as a bytes array
	public function ToArray():Bytes
	{
		return _bytes;
	}
}