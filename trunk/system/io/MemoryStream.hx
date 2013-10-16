package system.io;
import haxe.io.Bytes;

class MemoryStream
{
	
	var _data:Bytes;

	public function new(data:Bytes = null) 
	{
		_data = data;
	}
	
	public function ToArray():Bytes
	{
		return _data;
	}
	
}