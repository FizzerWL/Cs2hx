package system;

import system.Exception;

class Nullable_Int
{
	public var HasValue(HasValueGetter,null):Bool;
	public var Value(ValueGetter,null):Int;
	private var val:Int;
	private static var nullValue:Int = -2147483647; 

	public function new(initial:Int = -2147483647)
	{
		val = initial;
	}
	
	public function ValueGetter():Int
	{
		if (!HasValue)
			throw new Exception("Tried to access the value of a null Nullable_Int");
			
		return val;
	}
	
	public function HasValueGetter():Bool
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
