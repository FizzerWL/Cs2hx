package system.drawing;
import system.drawing.drawing2d.GraphicsPath;
import system.NotImplementedException;

class Graphics
{

	public function new() 
	{
		
	}
	
	public var InterpolationMode:Int;
	
	public function Dispose():Void
	{
		
	}
	
	public function Clear(c:Color):Void
	{
		throw new NotImplementedException();
	}
	
	public static function FromImage(bmp:Bitmap):Graphics
	{
		return throw new NotImplementedException();
	}
	
	public function FillRectangle_Brush_Single_Single_Single_Single(b:Brush, x:Float, y:Float, w:Float, h:Float):Void
	{
		throw new NotImplementedException();
	}
	
	public function DrawRectangle_Pen_Single_Single_Single_Single(p:Pen, x:Float, y:Float, w:Float, h:Float):Void
	{
		throw new NotImplementedException();
	}
	
	public function FillPath(brush:Brush, path:GraphicsPath):Void
	{
		throw new NotImplementedException();
	}
	public function DrawPath(pen:Pen, path:GraphicsPath):Void
	{
		throw new NotImplementedException();
	}
	
	public function DrawEllipse_Pen_Single_Single_Single_Single(p:Pen, x:Float, y:Float, xRad:Float, yRad:Float):Void
	{
		throw new NotImplementedException();
	}
	public function FillEllipse_Brush_Single_Single_Single_Single(b:Brush, x:Float, y:Float, xRad:Float, yRad:Float):Void
	{
		throw new NotImplementedException();
	}
	
	public function DrawImage_Image_Rectangle_Rectangle_GraphicsUnit(img:Image, dest:Rectangle, src:Rectangle, unit:Int):Void
	{
		throw new NotImplementedException();
	}
}