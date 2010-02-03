package system;

import system.Exception;

class Nullable_Float
{
	private var val:Float;
	private static var nullValue:Float = -2040404040; 
	
	public function new(initial:Float = -2040404040)
	{
		//TODO: Find a way for the constructor to differentiate between this special null value and its actual use.
		val = initial;
	}
	
	public var Value(get_Value, never):Float;
	public var HasValue(get_HasValue, never):Bool;
	
	public function get_Value():Float
	{
		if (!HasValue)
			throw new Exception("Tried to access the value of a null Nullable_Float");
			
		return val;
	}
	
	public function get_HasValue():Bool
	{
		return val != nullValue;
	}
	
	public function toString():String
	{
		if (!HasValue)
			return "";
		else
			return Std.string(val);
	}

}
