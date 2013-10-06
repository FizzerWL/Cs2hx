package system;

class DateTime
{
	public static var Now(get_Now,null):DateTime;
	public static var MaxValue(get_MaxValue,null):DateTime;
	public static var MinValue(get_MinValue,null):DateTime;

	public var Ticks(get_Ticks, null):Float;
	public var Year(get_Year, null):Int;
	public var Month(get_Month, null):Int;
	public var Day(get_Day, null):Int;
	
	public var date:Date;
	
	public function new(ticks:Float = -5)
	{
		if (ticks == -5)
			date = Date.now();
		else
			date = Date.fromTime(ticks);
	}
	
	public function Add(span:TimeSpan):DateTime
	{
		return new DateTime(date.getTime() + span.Ticks);
	}
	
	public function AddDays(days:Int):DateTime
	{
		return new DateTime(date.getTime() + TimeSpan.FromDays(days).Ticks);
	}
	
	public inline function toString():String
	{
		return date.getFullYear() + "/" + FormatDatePiece(date.getMonth() + 1) + "/" + FormatDatePiece(date.getDate()) + 
			" " + date.getHours() + ":" + FormatDatePiece(date.getMinutes()) + ":" + FormatDatePiece(date.getSeconds());
	}
	
	static function FormatDatePiece(n:Float):String
	{
		if (n < 10)
			return "0" + Std.string(n);
		else
			return Std.string(n);
	}

	
	public static function Parse(str:String):DateTime
	{
		var ret:DateTime = new DateTime();
		ret.date = Date.fromString(str);
		return ret;
	}
	
	//TODO: Ticks might not give the same value as .net does
	public inline function get_Ticks():Float
	{
		return date.getTime();
	}
	
	public inline function get_Year():Int
	{
		return date.getFullYear();
	}
	
	public inline function get_Month():Int
	{
		return date.getMonth();
	}
	
	public inline function get_Day():Int
	{
		return date.getDay();
	}

	
	public static inline function get_Now():DateTime 
	{
		return new DateTime();
	}
	
	public inline function Subtract(other:DateTime):TimeSpan
	{
		Assert.Fatal(other != null, "Subtract called with null date");
		return new TimeSpan(date.getTime() - other.date.getTime());
	}

	//TODO: Min and MaxValue might not give the same times as .net does
	public static inline function get_MinValue():DateTime
	{
		return new DateTime(0);
	}
	
	public static inline function get_MaxValue():DateTime
	{
		return new DateTime(3155378975999999999);
	}
}
