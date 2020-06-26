package system;
import system.globalization.CultureInfo;

class DateTime
{
	public static var Now(get,null):DateTime;
	public static var MaxValue(get,null):DateTime;
	public static var MinValue(get,null):DateTime;

	//public var Ticks(get, null):Float;
	public var Year(get, null):Int;
	public var Month(get, null):Int;
	public var Day(get, null):Int;
	public var Hour(get, null):Int;
	public var Minute(get, null):Int;
	public var Second(get, null):Int;
	public var Millisecond(get, null):Int;
	
	public var date:Date;
	
	public function new(first:Float = 0, second:Int = -1, third:Int = -1, forth:Int = -1, fifth:Int = -1, sixth:Int = -1)
	{
		if (first == 0)
			date = Date.fromTime(0);
		else if (first == -1)
			date = Date.now();
		else if (second != -1 && third != -1)
			date = new Date(Std.int(first), second - 1, third, forth, fifth, sixth);
		else
			date = Date.fromTime(first); //load it as milliseconds since epoch. Not the same as C#'s ctor but we can't match it since we don't support Ticks
	}
	
	public function Add(span:TimeSpan):DateTime
	{
		return new DateTime(date.getTime() + span.TotalMilliseconds);
	}
	public function Subtract_TimeSpan(span:TimeSpan):DateTime
	{
		return new DateTime(date.getTime() - span.TotalMilliseconds);
	}
	
	public inline function AddDays(days:Float):DateTime
	{
		return new DateTime(date.getTime() + TimeSpan.FromDays(days).TotalMilliseconds);
	}
	public inline function AddHours(hours:Float):DateTime
	{
		return new DateTime(date.getTime() + TimeSpan.FromHours(hours).TotalMilliseconds);
	}
	public inline function AddMinutes(minutes:Float):DateTime
	{
		return new DateTime(date.getTime() + TimeSpan.FromMinutes(minutes).TotalMilliseconds);
	}
	public inline function AddSeconds(seconds:Float):DateTime
	{
		return new DateTime(date.getTime() + TimeSpan.FromSeconds(seconds).TotalMilliseconds);
	}
	public inline function AddMilliseconds(ms:Float):DateTime
	{
		return new DateTime(date.getTime() + ms);
	}
	
	public inline function toString(format:String = null):String
	{
		if (format == null)
			return date.getFullYear() + "/" + FormatDatePiece(date.getMonth() + 1) + "/" + FormatDatePiece(date.getDate()) + 
				" " + date.getHours() + ":" + FormatDatePiece(date.getMinutes()) + ":" + FormatDatePiece(date.getSeconds());
		else
			throw new NotImplementedException();
	}
	
	public inline function ToShortDateString():String
	{
		return (date.getMonth() + 1) + "/" + date.getDate() + "/" + date.getFullYear();
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
	public static inline function Parse_String_IFormatProvider(str:String, culture:CultureInfo):DateTime
	{
		//TODO: Obey the culture
		return Parse(str);
	}

	/*TODO: Ticks doesn't give the same value as .net does.  Wouldn't be able to fix this without a proper bigint support since .net's Ticks won't fit in a Float with accuracy
	public inline function get_Ticks():Float
	{
		return date.getTime();
	}*/
	
	public function TotalMilliseconds():Float
	{
		var ret = date.getTime();
		if (Math.isNaN(ret))
			throw new Exception("DateTime is NaN");
		return ret;
	}
	
	public inline function get_Year():Int
	{
		return date.getFullYear();
	}
	
	public inline function get_Month():Int
	{
		return date.getMonth() + 1;
	}
	
	public inline function get_Day():Int
	{
		return date.getDate();
	}
	public inline function get_Hour():Int
	{
		return date.getHours();
	}
	public inline function get_Minute():Int
	{
		return date.getMinutes();
	}
	public inline function get_Second():Int
	{
		return date.getSeconds();
	}
	public inline function get_Millisecond():Int
	{
		return Std.int(date.getTime() % 1000);
	}

	
	public static inline function get_Now():DateTime 
	{
		return new DateTime(-1);
	}
	
	public inline function Subtract(other:DateTime):TimeSpan
	{
		Assert.Fatal(other != null, "Subtract called with null date");
		return new TimeSpan(date.getTime() - other.date.getTime());
	}

	public static inline function get_MinValue():DateTime
	{
		return new DateTime(0);
	}
	
	public static inline function get_MaxValue():DateTime
	{
		return new DateTime(3155378975999999999);
	}
	
	public static inline function op_Equality(t1:DateTime, t2:DateTime):Bool
	{
		return t1.date.getTime() == t2.date.getTime();
	}
	public static inline function op_Inquality(t1:DateTime, t2:DateTime):Bool
	{
		return t1.date.getTime() != t2.date.getTime();
	}
	public static inline function op_GreaterThan(t1:DateTime, t2:DateTime):Bool
	{
		return t1.date.getTime() > t2.date.getTime();
	}
	public static inline function op_LessThan(t1:DateTime, t2:DateTime):Bool
	{
		return t1.date.getTime() < t2.date.getTime();
	}
	public static inline function op_GreaterThanOrEqual(t1:DateTime, t2:DateTime):Bool
	{
		return t1.date.getTime() >= t2.date.getTime();
	}
	public static inline function op_LessThanOrEqual(t1:DateTime, t2:DateTime):Bool
	{
		return t1.date.getTime() <= t2.date.getTime();
	}
}
