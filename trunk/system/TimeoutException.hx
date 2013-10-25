package system;

class TimeoutException extends Exception
{

	public function new(msg:String = "No message", inner:Exception = null)
	{
		super(msg, inner);
	}
	
}