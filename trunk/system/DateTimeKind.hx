package system;

/**
 * ...
 * @author 
 */
class DateTimeKind
{
	public static inline var Utc:Int = 1;

	public static function ToString(k:Int):String
	{
		if (k == Utc)
			return "Utc";
		
		return throw new Exception("DateTimeKind");
	}
	
}