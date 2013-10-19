package system.io;
import haxe.io.Bytes;

class MemoryStream extends Stream
{
	
	var _data:Bytes;

	public function new(data:Bytes = null) 
	{
		super();
		_data = data;
	}
	
	public function ToArray():Bytes
	{
		return _data;
	}
	
}