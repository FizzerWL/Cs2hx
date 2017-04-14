package system;

class TypeCS
{

	public function new(obj:Dynamic) 
	{
		this.Name = Type.getClassName(Type.getClass(obj));
	}
	
	public var Name:String;
	
}