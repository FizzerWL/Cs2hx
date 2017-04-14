package system.drawing;
import system.NotImplementedException;

class Color
{

	public function new() 
	{
		
	}
	
	public var A:Int;
	public var R:Int;
	public var G:Int;
	public var B:Int;
	
	public static function FromArgb_Int32_Int32_Int32_Int32(a:Int, r:Int, g:Int, b:Int):Color
	{
		var c = new Color();
		c.R = r;
		c.G = g;
		c.B = b;
		c.A = a;
		return c;
	}
	
	public static function FromArgb_Int32_Int32_Int32(r:Int, g:Int, b:Int):Color
	{
		var c = new Color();
		c.R = r;
		c.G = g;
		c.B = b;
		c.A = 255;
		return c;
	}
	
	public static var White:Color;
	public static var Black:Color;
	public static var Green:Color;
	public static var Transparent:Color;
}