package system;

//If we name this Math, then there's no way to access Haxe's Math class.  So CS2HX renames Math on the fly to MathCS
class MathCS
{
	public function new() 
	{
	}
	
	public static inline function Max(f:Int, s:Int):Int
	{
		return f > s ? f : s;
	}
	public static inline function Max_Double_Double(f:Float, s:Float):Float
	{
		return f > s ? f : s;
	}
	public static inline function Max_Single_Single(f:Float, s:Float):Float
	{
		return f > s ? f : s;
	}
	public static inline function Max_Int64_Int64(f:Float, s:Float):Float
	{
		return f > s ? f : s;
	}
	
	
	public static inline function Min(f:Int, s:Int):Int
	{
		return f < s ? f : s;
	}
	public static inline function Min_Int64_Int64(f:Float, s:Float):Float
	{
		return f < s ? f : s;
	}
	public static inline function Min_Double_Double(f:Float, s:Float):Float
	{
		return f < s ? f : s;
	}
	public static inline function Min_Single_Single(f:Float, s:Float):Float
	{
		return f < s ? f : s;
	}
	
	public static inline function Abs(a:Int):Int
	{
		return a >= 0 ? a : -a;
	}
	public static inline function Abs_Single(a:Float):Float
	{
		return a >= 0 ? a : -a;
	}
	public static inline function Abs_Double(a:Float):Float
	{
		return a >= 0 ? a : -a;
	}
	
	public static inline function Log(a:Float):Float
	{
		return Math.log(a);
	}
	public static inline function Exp(a:Float):Float
	{
		return Math.exp(a);
	}
	public static inline function Sqrt(a:Float):Float
	{
		return Math.sqrt(a);
	}
	public static inline function Atan2(a:Float, b:Float):Float
	{
		return Math.atan2(a, b);
	}
	public static inline function Sin(a:Float):Float
	{
		return Math.sin(a);
	}
	public static inline function Cos(a:Float):Float
	{
		return Math.cos(a);
	}
	public static inline function Tan(a:Float):Float
	{
		return Math.tan(a);
	}
	public static inline function Asin(a:Float):Float
	{
		return Math.asin(a);
	}
	public static inline function Acos(a:Float):Float
	{
		return Math.acos(a);
	}
	public static inline function Atan(a:Float):Float
	{
		return Math.atan(a);
	}
	
	public static inline function Ceiling(a:Float):Float
	{
		return Math.fceil(a);
	}
	public static inline function Floor(a:Float):Float
	{
		return Math.ffloor(a);
	}

	public static inline var PI:Float = 3.1415926535897932384626433832795;
	
	public static inline function Pow(x:Float, y:Float):Float
	{
		return Math.pow(x, y);
	}
	public static inline function Round(a:Float):Float
	{
		return Math.round(a);
	}
}