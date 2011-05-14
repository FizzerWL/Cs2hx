package system;

import system.Exception;

class Nullable_TimeSpan
{
	public var HasValue(HasValueGetter,null):Bool;
	public var Value(ValueGetter,null):TimeSpan;
	private var val:TimeSpan;

	public function new(initial:TimeSpan = null)
	{
		val = initial;
	}
	
	public function ValueGetter():TimeSpan
	{
		if (!HasValue)
			throw new Exception("Tried to access the value of a null Nullable_TimeSpan");
			
		return val;
	}
	
	public function HasValueGetter():Bool
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
