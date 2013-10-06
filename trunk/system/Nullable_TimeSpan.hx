package system;

import system.Exception;

class Nullable_TimeSpan
{
	public var HasValue(get,null):Bool;
	public var Value(get,null):TimeSpan;
	private var val:TimeSpan;

	public function new(initial:TimeSpan = null)
	{
		val = initial;
	}
	
	public function get_Value():TimeSpan
	{
		if (!HasValue)
			throw new Exception("Tried to access the value of a null Nullable_TimeSpan");
			
		return val;
	}
	
	public function get_HasValue():Bool
	{
		return val != null;
	}
	
	public function toString():String
	{
		if (!HasValue)
			return "";
		else
			return val.toString();
	}

}
