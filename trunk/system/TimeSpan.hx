package system;

class TimeSpan
{
	public static var Zero(get,null):TimeSpan;

	public var Milliseconds(get,null):Int;
	public var Minutes(get,null):Int;
	//public var Ticks(get,null):Float;
	public var TotalMilliseconds(get,null):Float;
	public var Days(get,null):Int;
	public var Hours(get,null):Int;
	public var Seconds(get,null):Int;
	public var TotalSeconds(get,null):Float;
	public var TotalDays(get,null):Float;
	public var TotalHours(get,null):Float;
	public var TotalMinutes(get,null):Float;
	private var _totalMilliseconds : Float;
	
	public static var MaxValue:TimeSpan;
	public static var MinValue:TimeSpan;
	
	public static function cctor():Void
	{
		MaxValue = new TimeSpan(922337203685477);
		MinValue = new TimeSpan(-922337203685477);
	}
	
	public function toString():String
	{
		return Std.string(this.TotalMilliseconds);
	}
	
	public function new(milliseconds:Float = 0)
	{
		_totalMilliseconds = Math.floor(milliseconds);
	}
	
	public static inline function get_Zero():TimeSpan
	{
		return new TimeSpan(0);
	}
	
	/*
	public static inline function FromTicks(ticks:Float):TimeSpan
	{
		return new TimeSpan(ticks);
	}*/
	
	public function CompareTo_TimeSpan(span:TimeSpan):Int
	{
		return Std.int(this.TotalMilliseconds - span.TotalMilliseconds);
	}

	public inline function Subtract(other:TimeSpan):TimeSpan
	{
		return TimeSpan.FromMilliseconds(this.TotalMilliseconds - other.TotalMilliseconds);
	}

	/*  Ticks isn't the same as .net's Ticks, which we can't easily support without a bigint type.
	public inline function get_Ticks() : Float
	{
		return _totalMilliseconds;
	}*/
	
	public inline function get_Days() : Int
	{
		 return Std.int(_totalMilliseconds / MILLISECONDS_IN_DAY);
	}
	public inline function get_Hours() : Int
	{
		 return Std.int(_totalMilliseconds / MILLISECONDS_IN_HOUR) % 24;
	}
	public inline function get_Minutes() : Int
	{
		return Std.int(_totalMilliseconds / MILLISECONDS_IN_MINUTE) % 60; 
	}
	public inline function get_Seconds() : Int
	{
		return Std.int(_totalMilliseconds / MILLISECONDS_IN_SECOND) % 60;
	}
	public inline function get_Milliseconds() : Int
	{
		return Std.int(_totalMilliseconds) % 1000;
	}
	public inline function get_TotalDays() : Float
	{
		return _totalMilliseconds / MILLISECONDS_IN_DAY;
	}
	public inline function get_TotalHours() : Float
	{
		return _totalMilliseconds / MILLISECONDS_IN_HOUR;
	}
	public inline function get_TotalMinutes() : Float
	{
		return _totalMilliseconds / MILLISECONDS_IN_MINUTE;
	}
	public inline function get_TotalSeconds() : Float
	{
		return _totalMilliseconds / MILLISECONDS_IN_SECOND;
	}
	public inline function get_TotalMilliseconds() : Float
	{
		return _totalMilliseconds;
	}
	public function AddDate(date:Date):Date
	{
		var ms:Float = date.getTime();
		ms += TotalMilliseconds;
		return Date.fromTime(ms);
	}
	public function Add(span:TimeSpan):TimeSpan
	{
		return new TimeSpan(span.TotalMilliseconds + this.TotalMilliseconds);
	}
	public static function FromDates(start : Date, end : Date) : TimeSpan
	{
		return new TimeSpan(end.getTime() - start.getTime());
	}
	public static inline function FromMilliseconds(milliseconds : Float) : TimeSpan
	{
		return new TimeSpan(milliseconds);
	}
	public static inline function FromSeconds(seconds : Float) : TimeSpan
	{
		return new TimeSpan(seconds * MILLISECONDS_IN_SECOND);
	}
	public static inline function FromMinutes(minutes : Float) : TimeSpan
	{
		return new TimeSpan(minutes * MILLISECONDS_IN_MINUTE);
	}
	public static inline function FromHours(hours : Float) : TimeSpan
	{
		return new TimeSpan(hours * MILLISECONDS_IN_HOUR);
	}
	public static inline function FromDays(days : Float) : TimeSpan
	{
		return new TimeSpan(days * MILLISECONDS_IN_DAY);
	}
	

	public static inline var MILLISECONDS_IN_DAY : Float = 86400000;
	public static inline var MILLISECONDS_IN_HOUR : Float = 3600000;
	public static inline var MILLISECONDS_IN_MINUTE : Float = 60000;
	public static inline var MILLISECONDS_IN_SECOND : Float = 1000;
	
	public static inline function op_Equality(t1:TimeSpan, t2:TimeSpan):Bool
	{
		return t1.TotalMilliseconds == t2.TotalMilliseconds;
	}
	public static inline function op_Inequality(t1:TimeSpan, t2:TimeSpan):Bool
	{
		return t1.TotalMilliseconds != t2.TotalMilliseconds;
	}
	public static inline function op_GreaterThan(t1:TimeSpan, t2:TimeSpan):Bool
	{
		return t1.TotalMilliseconds > t2.TotalMilliseconds;
	}
	public static inline function op_LessThan(t1:TimeSpan, t2:TimeSpan):Bool
	{
		return t1.TotalMilliseconds < t2.TotalMilliseconds;
	}
	public static inline function op_GreaterThanOrEqual(t1:TimeSpan, t2:TimeSpan):Bool
	{
		return t1.TotalMilliseconds >= t2.TotalMilliseconds;
	}
	public static inline function op_LessThanOrEqual(t1:TimeSpan, t2:TimeSpan):Bool
	{
		return t1.TotalMilliseconds <= t2.TotalMilliseconds;
	}
}

