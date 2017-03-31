package system.drawing.drawing2d;
import system.NotImplementedException;

class GraphicsPath
{
	public var FillMode:Int;

	public function new() 
	{
		
	}
	
	public function AddLine_Single_Single_Single_Single(x1:Float, y1:Float, x2:Float, y2:Float):Void
	{
		throw new NotImplementedException();
	}
	
	public function AddArc_Single_Single_Single_Single_Single_Single(x:Float, y:Float, w:Float, h:Float, startAngle:Float, sweepAngle:Float):Void
	{
		throw new NotImplementedException();
	}
	
	public function CloseFigure():Void
	{
		throw new NotImplementedException();
	}
	
	public function AddBezier_Single_Single_Single_Single_Single_Single_Single_Single(x1:Float, y1:Float, x2:Float, y2:Float, x3:Float, y3:Float, x4:Float, y4:Float):Void
	{
		throw new NotImplementedException();
	}
}