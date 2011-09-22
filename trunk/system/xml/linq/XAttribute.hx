package system.xml.linq;

//XAttribute here is just a simple wrapper around the .Value.  This isn't true to .net, but it's a shortcut which works for our needs at this time.
class XAttribute extends XObject
{
	public var Value:String;
	
	public function new(val:String)
	{
		Value = val;
	}
}