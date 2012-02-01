package system.diagnostics;

import system.Environment;
import system.NotImplementedException;
import system.TimeSpan;

class Stopwatch 
{
	var _started:Float;
	
	public static function StartNew():Stopwatch
	{
		return new Stopwatch();
	}
	
	public var Elapsed(get_Elapsed, never):TimeSpan;
	
	public function get_Elapsed():TimeSpan
	{
		var now = Environment.GetTickCount();
		return TimeSpan.FromMilliseconds(now - _started);
	}
	
	public function Restart():Void
	{
		_started = Environment.GetTickCount();
	}

	private function new() 
	{
		_started = Environment.GetTickCount(); //This always returns milliseconds
	}
	
}