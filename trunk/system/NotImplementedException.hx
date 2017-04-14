package system;
class NotImplementedException extends Exception
{
	public function new(msg:String = "Not implemented")
	{
		super(msg);
	}

}
