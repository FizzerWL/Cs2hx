package system.threading.tasks;
import system.NotImplementedException;

class TaskFactory
{

	public function new() 
	{
		
	}
	
	public function StartNew<T>(fn:Void->T):Task<T>
	{
		return throw new NotImplementedException();
	}
	
}