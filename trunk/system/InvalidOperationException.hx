package system;

class InvalidOperationException extends Exception
{
	public function new(msg:String = "Invalid operation")
	{
		super(msg);
	}

}
