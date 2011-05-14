package system;

import system.Exception;

class Nullable_DateTime
{
	public var HasValue(HasValueGetter,null):Bool;
	public var Value(ValueGetter,null):DateTime;
	private var val:DateTime;

	public function new(initial:DateTime = null)
	{
		val = initial;
	}
	
	public function ValueGetter():DateTime
	{
		if (!HasValue)
			throw new Exception("Tried to access the value of a null Nullable_DateTime");
			
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
