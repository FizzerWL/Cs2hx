package system.net;
import system.io.Stream;
import system.NotImplementedException;

class HttpWebResponse extends WebResponse
{

	public function new() 
	{
		super();
	}

	public var StatusCode:Int;
	public var Cookies:CookieContainer;
	
}