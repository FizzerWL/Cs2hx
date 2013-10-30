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
	
	public function new(first:Float = -5, second:Int = -1, third:Int = -1)
	{
		if (first == -1)
			date = Date.now();
		else if (second != -1 && third != -1)
			date = new Date(Std.int(first), second - 1, third, 0, 0, 0);
		else
			date = Date.fromTime(first);
	}
	
	public function Add(span:TimeSpan):DateTime
	{
		return new DateTime(date.getTime() + span.Ticks);
	}
	public function Subtract_TimeSpan(span:TimeSpan):DateTime
	{
		return new DateTime(date.getTime() - span.Ticks);
	}
	
	public inline function AddDays(days:Float):DateTime
	{
		return new DateTime(date.getTime() + TimeSpan.FromDays(days).Ticks);
	}
	public inline function AddHours(hours:Float):DateTime
	{
		return new DateTime(date.getTime() + TimeSpan.FromHours(hours).Ticks);
	}
	public inline function AddMinutes(minutes:Float):DateTime
	{
		return new DateTime(date.getTime() + TimeSpan.FromMinutes(minutes).Ticks);
	}
	public inline function AddSeconds(seconds:Float):DateTime
	{
		return new DateTime(date.getTime() + TimeSpan.FromSeconds(seconds).Ticks);
	}
	
	public function ToLocalTime():DateTime
	{
		return throw new NotImplementedException();
	}

	public inline function toString(format:String = null):String
	{
		if (format == null)
			return date.getFullYear() + "/" + FormatDatePiece(date.getMonth() + 1) + "/" + FormatDatePiece(date.getDay()) + 
				" " + date.getHours() + ":" + FormatDatePiece(date.getMinutes()) + ":" + FormatDatePiece(date.getSeconds());
		else
			throw new NotImplementedException();
	}
	
	public inline function ToShortDateString():String
	{
		return date.getFullYear() + "/" + (date.getMonth() + 1) + "/" + date.getDay();
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
