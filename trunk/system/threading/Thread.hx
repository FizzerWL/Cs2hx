package system.threading;
import system.NotImplementedException;

class Thread
{

	public function new() 
	{
		
	}
	
	public static function Sleep(ms:Int):Void
	{
		Sys.sleep(ms / 1000);
	}
	
}