package system;

import system.Exception;

class Nullable<T>
{
	private var val:T;
	
	public function new(initial:T = null)
	{
		val = initial;
	}
	
	public var Value(get_Value, never):T;
	public var HasValue(get_HasValue, never):Bool;
	
	public function get_Value():T
	{
		if (!HasValue)
			throw new Exception("Tried to access the value of a null Nullable");
			
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
			return Std.string(val);
	}

}
