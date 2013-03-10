package system;

class TimeSpan
{
	public static var Zero(ZeroGetter,null):TimeSpan;

	public var Milliseconds(MillisecondsGetter,null):Int;
	public var Minutes(MinutesGetter,null):Int;
	public var Ticks(TicksGetter,null):Float;
	public var TotalMilliseconds(TotalMillisecondsGetter,null):Float;
	public var Days(DaysGetter,null):Int;
	public var Hours(HoursGetter,null):Int;
	public var Seconds(SecondsGetter,null):Int;
	public var TotalSeconds(TotalSecondsGetter,null):Float;
	public var TotalDays(TotalDaysGetter,null):Float;
	public var TotalHours(TotalHoursGetter,null):Float;
	public var TotalMinutes(TotalMinutesGetter,null):Float;
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
		return Std.string(this.Ticks);
	}
	
	public function new(milliseconds : Float)
	{
		_totalMilliseconds = Math.floor(milliseconds);
	}
	
	public static inline function ZeroGetter():TimeSpan
	{
		return new TimeSpan(0);
	}
	
	public static inline function FromTicks(ticks:Float):TimeSpan
	{
		return new TimeSpan(ticks);
	}
	
	public inline function Subtract(other:TimeSpan):TimeSpan
	{
		return TimeSpan.FromTicks(this.TicksGetter() - other.TicksGetter());
	}
	
	public inline function TicksGetter() : Float
	{
		return _totalMilliseconds;
	}
	
	public inline function DaysGetter() : Int
	{
		 return Std.int(_totalMilliseconds / MILLISECONDS_IN_DAY);
	}
	public inline function HoursGetter() : Int
	{
		 return Std.int(_totalMilliseconds / MILLISECONDS_IN_HOUR) % 24;
	}
	public inline function MinutesGetter() : Int
	{
		return Std.int(_totalMilliseconds / MILLISECONDS_IN_MINUTE) % 60; 
	}
	public inline function SecondsGetter() : Int
	{
		return Std.int(_totalMilliseconds / MILLISECONDS_IN_SECOND) % 60;
	}
	public inline function MillisecondsGetter() : Int
	{
		return Std.int(_totalMilliseconds) % 1000;
	}
	public inline function TotalDaysGetter() : Float
	{
		return _totalMilliseconds / MILLISECONDS_IN_DAY;
	}
	public inline function TotalHoursGetter() : Float
	{
		return _totalMilliseconds / MILLISECONDS_IN_HOUR;
	}
	public inline function TotalMinutesGetter() : Float
	{
		return _totalMilliseconds / MILLISECONDS_IN_MINUTE;
	}
	public inline function TotalSecondsGetter() : Float
	{
		return _totalMilliseconds / MILLISECONDS_IN_SECOND;
	}
	public inline function TotalMillisecondsGetter() : Float
	{
		return _totalMilliseconds;
	}
	public function Add(date : Date) : Date
	{
		var ms:Float = date.getTime();
		ms += TotalMilliseconds;
		return Date.fromTime(ms);
	}
	public function AddSpan(span:TimeSpan):TimeSpan
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

	public static var MILLISECONDS_IN_DAY : Float = 86400000;
	public static var MILLISECONDS_IN_HOUR : Float = 3600000;
	public static var MILLISECONDS_IN_MINUTE : Float = 60000;
	public static var MILLISECONDS_IN_SECOND : Float = 1000;
}

