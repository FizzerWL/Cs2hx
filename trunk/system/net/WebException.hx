package system.net;
import system.Exception;

class WebException extends Exception
{

	public function new(msg:String = null, inner:Exception = null) 
	{
		super(msg, inner);
	}
	
	public var Response:WebResponse;
}