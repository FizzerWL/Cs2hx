package system.web;
import system.NotImplementedException;

class HttpCookieCollection
{

	public function new() 
	{
		
	}
	
	public function GetValue_String(name:String):HttpCookie
	{
		return throw new NotImplementedException();
	}
	
	public function Remove(name:String):Void
	{
		throw new NotImplementedException();
	}
	public function Add(cookie:HttpCookie):Void
	{
		throw new NotImplementedException();
	}
	
}