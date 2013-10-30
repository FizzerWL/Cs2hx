package system.drawing.drawing2d;
import system.drawing.PointF;
import system.NotImplementedException;

class Matrix
{

	public function new(m11:Float = 0, m12:Float = 0, m21:Float = 0, m22:Float = 0, dx:Float = 0, dy:Float = 0) 
	{
		throw new NotImplementedException();
	}
	
	public function Translate(x:Float, y:Float):Void
	{
		
	}
	
	public function Scale(x:Float, y:Float):Void
	{
		
	}
	public function Rotate(angle:Float):Void
	{
		
	}
	
	public function TransformPoints(p:Array<PointF>):Void
	{
		
	}
	
	public function TransformVectors(p:Array<PointF>):Void
	{
		
	}
	
	public function Multiply(m:Matrix):Void
	{
		
	}

	public var IsIdentity:Bool;
	
}