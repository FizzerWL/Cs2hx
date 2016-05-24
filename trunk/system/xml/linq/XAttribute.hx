package system.xml.linq;
import system.NotImplementedException;

class XAttribute extends XObject
{
	public var Value:String;
	public var Name:XName;
	
	public function new(name:String, val:String)
	{
		super();
		Name = new XName(name);
		Value = val;
	}
	
	public function Remove():Void
	{
		throw new NotImplementedException();
	}
}