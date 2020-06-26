package system;

class TypeCS
{

	public function new(obj:Dynamic) 
	{
		this.FullName = Type.getClassName(Type.getClass(obj));
		
		var i = this.FullName.lastIndexOf(".");
		this.Name = i == -1 ? this.FullName : this.FullName.substr(i + 1);
	}

	public var FullName:String;
	public var Name:String;
	
}