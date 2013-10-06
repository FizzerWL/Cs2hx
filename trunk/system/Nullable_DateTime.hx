package system;

import system.Exception;

class Nullable_DateTime
{
	public var HasValue(get,null):Bool;
	public var Value(get,null):DateTime;
	private var val:DateTime;

	public function new(initial:DateTime = null)
	{
		val = initial;
	}
	
	public function get_Value():DateTime
	{
		if (!HasValue)
			throw new Exception("Tried to access the value of a null Nullable_DateTime");
			
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
