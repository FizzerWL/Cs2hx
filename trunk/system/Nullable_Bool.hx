package system;

import system.Exception;

class Nullable_Bool
{
	private var val:Int; //-1 for null, 0 for false, 1 for true
	
	public var HasValue(get,null):Bool;
	public var Value(get,null):Bool;

	public function new(initial:Dynamic = null)
	{
		if (initial == null)
			val = -1;
		else
			val = initial ? 1 : 0;
	}
	
	public function get_Value():Bool
	{
		if (!HasValue)
			throw new Exception("Tried to access the value of a null Nullable_Bool");
			
		return val == 1;
	}
	
	public function get_HasValue():Bool
	{
		return val != -1;
	}
	
	public function toString():String
	{
		if (!HasValue)
			return "";
		else
			return Std.string(val);
	}

}
