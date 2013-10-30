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
		return throw new NotImplementedException();
	}
	
	public static function FromArgb_Int32_Int32_Int32(r:Int, g:Int, b:Int):Color
	{
		return throw new NotImplementedException();
	}
	
	public static var White:Color;
	public static var Black:Color;
	public static var Green:Color;
	public static var Transparent:Color;
}