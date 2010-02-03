package system;

import flash.Error;

class Exception extends Error
{
	public function new(msg:String = "No message")
	{
		super(msg);
	}

	public function toString():String
	{
		return "Exception: " + name + ": " + message;
	}
}
