package system.xml.linq;
import system.NotImplementedException;

class XElement extends XContainer
{
	public function new(name:String = null)
	{
		super(name);
	}
	
	public function GetDefaultNamespace():XNamespace
	{
		return throw new NotImplementedException();
	}
	
	public var NodeType:Int;
	
}