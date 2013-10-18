package system.collections.specialized;
import system.NotImplementedException;

class NameValueCollection
{

	public function new() 
	{
		
	}
	
	public var Keys (get, never):Array<String>;
	public function get_Keys():Array<String>
	{
		return throw new NotImplementedException();
	}
	
	public var Values (get, never):Array<String>;
	public function get_Values():Array<String>
	{
		return throw new NotImplementedException();
	}
	
	public function GetValue(k:String):String
	{
		return throw new NotImplementedException();
	}
}