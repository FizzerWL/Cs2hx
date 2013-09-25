package system;

import system.Exception;

//This class is somewhat redundant since strings are already nullable, however this is used by CS2HX in rare cases where a non-nullable type is swapped for a nullable type
class Nullable_String
{
	private var val:String;
	
	public function new(initial:String = null)
	{
		val = initial;
	}
	
	public var Value(get_Value, never):String;
	public var HasValue(get_HasValue, never):Bool;
	
	public function get_Value():String
	{
		if (!HasValue)
			throw new Exception("Tried to access the value of a null Nullable_String");
			
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
