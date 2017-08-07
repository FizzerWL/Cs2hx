package system;

class AggregateException extends Exception
{

	public function new(exceptions:Array<Exception>) 
	{
		var s = "";
		
		for (e in exceptions)
			s += e.toString() + "\n\n";
		super(s);
	}
	
	
}