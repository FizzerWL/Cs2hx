package system.web;
import system.DateTime;
import system.NotImplementedException;

class HttpCookie
{

	public function new(name:String, val:String = null) 
	{
		throw new NotImplementedException();
	}
	
	public var Value:String;
	public var Expires:DateTime;
	
}