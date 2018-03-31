package system;

import system.Exception;

class Nullable<T>
{
	public var ValueOpt:T;
	
	public function new(initial:T = null)
	{
		ValueOpt = initial;
	}
	
	public var Value(get, never):T;
	public var HasValue(get, never):Bool;
	
	public function get_Value():T
	{
		if (!HasValue)
			throw new Exception("Tried to access the value of a null Nullable");
			
		return ValueOpt;
	}
	
	public function get_HasValue():Bool
	{
		return ValueOpt != null;
	}
	
	public function toString():String
	{
		if (!HasValue)
			return "";
		else
			return Std.string(ValueOpt);
	}
	
}
