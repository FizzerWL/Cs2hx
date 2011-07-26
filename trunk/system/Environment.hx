package system;

import haxe.Timer;

#if flash
import flash.Lib;
#end

class Environment 
{

	public function new() 
	{
		
	}
	
	public static var TickCount(GetTickCount, never):Float;
	
	public static function GetTickCount():Float
	{
		#if flash
		return Lib.getTimer();
		#else
		return Std.int(Timer.stamp());
		#end
	}
	
}