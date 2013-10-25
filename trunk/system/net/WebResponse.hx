package system.net;
import system.io.Stream;

class WebResponse
{

	public function new() 
	{
		
	}
	
	public var ContentLength:Int;
	public function GetResponseStream():Stream
	{
		return throw new NotImplementedException();
	}
	
	public function Dispose():Void
	{
		
	}
}