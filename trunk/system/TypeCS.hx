package system;

class TypeCS
{

	public function new(obj:Dynamic) 
	{
		this.Name = Type.getClassName(obj);
	}
	
	public var Name:String;
	
}