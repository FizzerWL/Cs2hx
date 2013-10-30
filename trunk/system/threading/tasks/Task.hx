package system.threading.tasks;
import system.NotImplementedException;

class Task<T>
{

	public function new() 
	{
		
	}
	
	public var Result:T;

	
	public function Wait():Void
	{
		throw new NotImplementedException();
	}
	
	public static var Factory:TaskFactory;
}