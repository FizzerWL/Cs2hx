package system.diagnostics;

import system.NotImplementedException;

//Stopwatch is stubbed out so that code that uses it compiles, however it is not implemented and always returns 0 duration
class Stopwatch 
{
	public static function StartNew():Stopwatch
	{
		return new Stopwatch();
	}

	private function new() 
	{
		
	}
	
}