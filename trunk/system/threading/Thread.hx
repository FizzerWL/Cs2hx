package system.threading;
import system.NotImplementedException;

class Thread
{

	public function new(fn:Dynamic->Void) 
	{
		throw new NotImplementedException();
	}
	
	public var Name:String;
	public var ManagedThreadId:Int;
	public var IsAlive:Bool;
	
	public function Start():Void
	{
		throw new NotImplementedException();
	}
	
	public static function Sleep(ms:Int):Void
	{
		#if (flash || js)
		throw new NotImplementedException();
		#else
		Sys.sleep(ms / 1000);
		#end
	}
	
	public static var CurrentThread:Thread;
	
	public function SetApartmentState(state:Int):Void
	{
		throw new NotImplementedException();
	}
	
	public function Abort():Void
	{
		throw new NotImplementedException();
	}
	
}