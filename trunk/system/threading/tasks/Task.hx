package system.threading.tasks;
import system.Exception;
import system.NotImplementedException;

class Task<T>
{

	public function new(fn:Void->T) 
	{
		throw new NotImplementedException();
	}
	
	public var Result:T;
	public var IsCompleted:Bool;
	public var IsFaulted:Bool;
	public var Exception:Exception;
	
	public function Wait():Void
	{
		throw new NotImplementedException();
	}
	
	public function Start():Void
	{
		
	}
	
	public static var Factory:TaskFactory;
}