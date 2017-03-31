package system;

import system.Exception;

class Nullable_Int
{
	public var HasValue(get,null):Bool;
	public var Value(get,null):Int;
	private var val:Int;
	private static var nullValue:Int = -2147483647; 

	public function new(initial:Int = -2147483647)
	{
		val = initial;
	}
	
	public function get_Value():Int
	{
		if (!HasValue)
			throw new Exception("Tried to access the value of a null Nullable_Int");
			
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
