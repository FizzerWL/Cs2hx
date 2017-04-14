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
	
	public var Elapsed(get, never):TimeSpan;
	
	public function get_Elapsed():TimeSpan
	{
		return TimeSpan.FromMilliseconds(Environment.TickCount - _started);
	}
	
	public function Restart():Void
	{
		_started = Environment.TickCount;
	}

	private function new() 
	{
		_started = Environment.TickCount; //This always returns milliseconds
	}
	
	public var ElapsedMilliseconds(get, never):Float;
	public function get_ElapsedMilliseconds():Float
	{
		return Environment.TickCount - _started;
	}
}