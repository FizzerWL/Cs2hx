package system.web;
import system.collections.generic.Dictionary;

class HttpContext
{

	public function new() 
	{
		
	}
	
	public static var Current:HttpContext;
	public var Request:HttpRequest;
	public var Response:HttpResponse;
	public var Items:Dictionary<String, Dynamic>;
}