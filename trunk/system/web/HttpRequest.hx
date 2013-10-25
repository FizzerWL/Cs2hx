package system.web;
import system.NotImplementedException;
import system.Uri;

class HttpRequest
{

	public function new() 
	{
		throw new NotImplementedException();
	}
	
	public var UserAgent:String;
	public var UrlReferrer:Uri;
	public var Cookies:HttpCookieCollection;
}