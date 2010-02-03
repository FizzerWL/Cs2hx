package system;
import flash.Error;
import system.Cs2Hx;

class Guid
{		
	public static function NewGuid():String
	{
		//This is not a real guid generation algorithm.  This just generates 16 random bytes, which is enough for some uses.
		var ret:String = "";
		
		for(i in 0...32)
			ret += Cs2Hx.CharToHex(Std.random(16));
		
		if (ret.length != 32)
			throw new Error();
		return ret;
	}
	
	
}