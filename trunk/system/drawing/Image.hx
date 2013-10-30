package system.drawing;
import system.drawing.imaging.ImageFormat;
import system.IntPtr;
import system.io.Stream;
import system.NotImplementedException;

class Image
{

	public function new() 
	{
		
	}
	
	public static function FromStream(s:Stream):Image
	{
		return throw new NotImplementedException();
	}
	
	public var Width:Int;
	public var Height:Int;
	
	public function RotateFlip(type:Int):Void
	{
		throw new NotImplementedException();
	}
	
	public function GetThumbnailImage(width:Int, height:Int, abort:Void->Bool, callbackData:IntPtr):Image
	{
		return throw new NotImplementedException();
	}
	
	public function Save_Stream_ImageFormat(s:Stream, format:ImageFormat):Void
	{
		throw new NotImplementedException();
	}
}