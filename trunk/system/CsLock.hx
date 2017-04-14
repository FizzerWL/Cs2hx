package system;


#if java
import java.Lib;
#end

class CsLock
{

	public function new() 
	{
	}
	
	public static function Lock(obj:Dynamic, action:Void->Void):Void
	{
		//Lock is currently only supported in java.  Other platforms just execute the code, which is fine for single-threaded platforms such as actionscript.
		#if java
		Lib.lock(obj, {
			action();
			1;
		});
		#else
		action();
		#end
	}
}