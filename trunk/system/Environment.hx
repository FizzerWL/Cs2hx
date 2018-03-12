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
	
	public static var TickCount(get, never):Int;
	
	public static function get_TickCount():Int
	{
		#if flash
		return Lib.getTimer();
		#else
		return Std.int(Timer.stamp() * 1000);
		#end
	}
	
	public static var OSVersion (get, never):Dynamic;
	public static function get_OSVersion():Dynamic
	{
		return throw new NotImplementedException();
	}

	public static inline var NewLine:String = "\n";
}