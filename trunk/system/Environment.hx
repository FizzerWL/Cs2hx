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

	private static var _pausedAt:Int = -1;
	
	public static function get_TickCount():Int
	{
		var ret = Std.int(Timer.stamp() * 1000);

		var ticking = js.Syntax.code("window['Ticking']");
		if (ticking != null) {
			var num = Std.parseFloat(ticking);
			if (num == 0) {
				if (_pausedAt == -1)
					_pausedAt = ret;
				return _pausedAt;
			}
			else {
				return Std.int(ret / num);
			}

		}

		return ret;
	}
	
	public static var OSVersion (get, never):Dynamic;
	public static function get_OSVersion():Dynamic
	{
		return throw new NotImplementedException();
	}

	public static inline var NewLine:String = "\n";
	public static inline var StackTrace = ""; //not available in haxe
}