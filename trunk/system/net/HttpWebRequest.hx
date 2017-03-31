package system.net;
import system.io.Stream;
import system.NotImplementedException;
import system.Uri;

class HttpWebRequest extends WebRequest
{

	public function new() 
	{
		super();
	}
	
	public static function Create(url:String):HttpWebRequest
	{
		return throw new NotImplementedException();
	}
	public static function Create_Uri(uri:Uri):HttpWebRequest
	{
		return throw new NotImplementedException();
	}
	
	public var Method:String;
	public var ContentType:String;
	public var Timeout:Int;
	public var ContentLength:Int;
	public var CookieContainer:CookieContainer;
	
	public function GetRequestStream():Stream
	{
		return throw new NotImplementedException();
	}
	
	public function GetResponse():HttpWebResponse
	{
		return throw new NotImplementedException();
	}
	
}