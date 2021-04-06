package system;

/**
 * ...
 * @author 
 */
class DateTimeKind
{
	public static inline var Unspecified:Int = 0;
	public static inline var Utc:Int = 1;
	public static inline var Local:Int = 2;

	public static function ToString(k:Int):String
	{
		if (k == Unspecified)
			return "Unspecified";
		if (k == Utc)
			return "Utc";
		if (k == Local)
			return "Local";
		
		return Std.string(k);
		
	}
	
}