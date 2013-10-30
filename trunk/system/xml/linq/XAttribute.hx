package system.xml.linq;

class XAttribute extends XObject
{
	public var Value:String;
	public var Name:XName;
	
	public function new(val:String)
	{
		super();
		Value = val;
	}
}