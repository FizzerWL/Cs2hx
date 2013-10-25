package system.runtime.caching;
import system.NotImplementedException;

class MemoryCache
{

	public function new(str:String) 
	{
		
	}

	public function Get(key:String):Dynamic
	{
		return throw new NotImplementedException();
	}
	
	public function GetValue_String(str:String):Dynamic
	{
		return throw new NotImplementedException();
	}
	
	public function Remove(str:String):Void
	{
		throw new NotImplementedException();
	}
	
	public function Add_String_Object_CacheItemPolicy_String(key:String, obj:Dynamic, policy:CacheItemPolicy):Void
	{
		throw new NotImplementedException();
	}
	
	
}